import 'package:dnd_app/core/errors/app_exception.dart';
import 'package:dnd_app/core/api/models/common_models.dart';
import 'package:dnd_app/core/api/models/sales_models.dart';
import 'package:dnd_app/core/auth/role_permissions.dart';
import 'package:dnd_app/core/auth/session_controller.dart';
import 'package:dnd_app/core/ui/ui.dart';
import 'package:dnd_app/core/utils/currency_utils.dart';
import 'package:dnd_app/core/utils/money_formatter.dart';
import 'package:dnd_app/core/utils/world_date_formatter.dart';
import 'package:dnd_app/features/campaigns/campaign_providers.dart';
import 'package:dnd_app/features/sales/sales_providers.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

class SalesListPage extends ConsumerWidget {
  const SalesListPage({super.key, required this.campaignId});

  final String campaignId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final page = ref.watch(salesPageProvider(campaignId));
    final homePage = ref.watch(campaignHomePageProvider(campaignId));
    final home = homePage.valueOrNull;
    final salesCurrency = home?.currency == null
        ? null
        : currencyWithDndCoinFallbacks(home!.currency);
    final session = ref.watch(sessionControllerProvider);
    final canWrite =
        (session.user?.isPlatformAdmin ?? false) ||
        isCampaignWriteRole(home?.myRole);

    ref.listen<AsyncValue<void>>(salesDraftControllerProvider, (
      previous,
      next,
    ) {
      next.whenOrNull(
        error: (error, _) {
          final message = error is AppException
              ? error.message
              : 'Sales action failed.';
          ScaffoldMessenger.of(
            context,
          ).showSnackBar(SnackBar(content: Text(message)));
        },
      );
    });

    return DefaultTabController(
      length: 3,
      child: AppScaffold(
        title: 'Sales',
        actions: [
          IconButton(
            onPressed: () => context.go('/campaign/$campaignId/home'),
            icon: const Icon(Icons.home_outlined),
          ),
        ],
        floatingActionButton: canWrite
            ? FloatingActionButton.extended(
                onPressed: () async {
                  try {
                    final draftId = await ref
                        .read(salesDraftControllerProvider.notifier)
                        .createDirectSale(campaignId);
                    if (context.mounted) {
                      context.push(
                        '/campaign/$campaignId/sales/checkout/$draftId',
                      );
                    }
                  } catch (_) {
                    // Errors are surfaced through provider listener.
                  }
                },
                icon: const Icon(Icons.add),
                label: const Text('New Sale'),
                extendedPadding: const EdgeInsets.symmetric(horizontal: 26),
              )
            : null,
        child: AsyncPage(
          value: page,
          onRetry: () => ref.invalidate(salesPageProvider(campaignId)),
          onRefresh: () => ref.refresh(salesPageProvider(campaignId).future),
          builder: (data) {
            final drafts = data.sales
                .where((sale) => sale.status.toLowerCase() == 'draft')
                .toList();
            final completed = data.sales
                .where((sale) => sale.status.toLowerCase() == 'completed')
                .toList();
            final voided = data.sales
                .where((sale) => sale.status.toLowerCase() == 'voided')
                .toList();

            return Column(
              children: [
                Padding(
                  padding: const EdgeInsets.fromLTRB(16, 16, 16, 8),
                  child: DecoratedBox(
                    decoration: BoxDecoration(
                      color: FantasyColors.panel,
                      borderRadius: BorderRadius.circular(14),
                      border: Border.all(color: FantasyColors.border),
                    ),
                    child: TabBar(
                      dividerColor: Colors.transparent,
                      indicatorSize: TabBarIndicatorSize.tab,
                      indicatorPadding: const EdgeInsets.all(6),
                      indicator: BoxDecoration(
                        color: FantasyColors.teal.withValues(alpha: 0.10),
                        borderRadius: BorderRadius.circular(10),
                        border: Border.all(
                          color: FantasyColors.teal.withValues(alpha: 0.32),
                        ),
                      ),
                      tabs: const [
                        Tab(icon: Icon(Icons.draw_outlined), text: 'Draft'),
                        Tab(
                          icon: Icon(Icons.check_circle_outline),
                          text: 'Completed',
                        ),
                        Tab(icon: Icon(Icons.block), text: 'Void'),
                      ],
                    ),
                  ),
                ),
                Expanded(
                  child: TabBarView(
                    children: [
                      _SalesTabList(
                        campaignId: campaignId,
                        rows: drafts,
                        currency: salesCurrency,
                        calendar: home?.calendar,
                        emptyTitle: 'No drafts',
                        emptyText:
                            'Create a new draft to get started managing your sales.',
                      ),
                      _SalesTabList(
                        campaignId: campaignId,
                        rows: completed,
                        currency: salesCurrency,
                        calendar: home?.calendar,
                        emptyTitle: 'No completed sales',
                        emptyText:
                            'Completed sales will appear here once a draft is finished.',
                      ),
                      _SalesTabList(
                        campaignId: campaignId,
                        rows: voided,
                        currency: salesCurrency,
                        calendar: home?.calendar,
                        emptyTitle: 'No voided sales',
                        emptyText:
                            'Voided sales will be kept here for campaign records.',
                      ),
                    ],
                  ),
                ),
              ],
            );
          },
        ),
      ),
    );
  }
}

class _SalesTabList extends StatelessWidget {
  const _SalesTabList({
    required this.campaignId,
    required this.rows,
    required this.currency,
    required this.calendar,
    required this.emptyTitle,
    required this.emptyText,
  });

  final String campaignId;
  final List<SalesPageRowDto> rows;
  final CurrencyConfigDto? currency;
  final CalendarConfigDto? calendar;
  final String emptyTitle;
  final String emptyText;

  @override
  Widget build(BuildContext context) {
    if (rows.isEmpty) {
      return FantasyEmptyState(
        title: emptyTitle,
        message: emptyText,
        variant: FantasyEmptyVariant.scroll,
      );
    }

    return ListView.builder(
      padding: const EdgeInsets.all(16),
      itemCount: rows.length,
      itemBuilder: (context, index) {
        final row = rows[index];
        final isDraft = row.status.toLowerCase() == 'draft';
        final totalText = currency == null
            ? '${row.totalMinor}'
            : formatMoneyMinorUnits(row.totalMinor, currency!);
        final dateText = calendar == null
            ? 'Day ${row.soldWorldDay}'
            : formatWorldDate(worldDay: row.soldWorldDay, calendar: calendar!);
        return Padding(
          padding: const EdgeInsets.only(bottom: 10),
          child: FantasyPanel(
            padding: EdgeInsets.zero,
            child: ListTile(
              title: Text(row.customerName ?? 'Walk-in'),
              subtitle: Text('$dateText\nTotal: $totalText'),
              isThreeLine: true,
              trailing: Text(row.status),
              onTap: () => context.push(
                isDraft
                    ? '/campaign/$campaignId/sales/checkout/${row.saleId}'
                    : '/campaign/$campaignId/sales/${row.saleId}',
              ),
            ),
          ),
        );
      },
    );
  }
}
