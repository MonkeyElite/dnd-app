import 'package:dnd_app/core/errors/app_exception.dart';
import 'package:dnd_app/core/api/models/sales_models.dart';
import 'package:dnd_app/core/ui/ui.dart';
import 'package:dnd_app/features/sales/sales_providers.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

class SalesListPage extends ConsumerWidget {
  const SalesListPage({
    super.key,
    required this.campaignId,
  });

  final String campaignId;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final page = ref.watch(salesPageProvider(campaignId));

    ref.listen<AsyncValue<void>>(salesDraftControllerProvider, (previous, next) {
      next.whenOrNull(
        error: (error, _) {
          final message = error is AppException ? error.message : 'Sales action failed.';
          ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(message)));
        },
      );
    });

    return DefaultTabController(
      length: 3,
      child: AppScaffold(
        title: 'Sales',
        floatingActionButton: FloatingActionButton.extended(
          onPressed: () async {
            try {
              final draftId = await ref.read(salesDraftControllerProvider.notifier).createDraft(campaignId);
              if (context.mounted) {
                context.go('/campaign/$campaignId/sales/draft/$draftId');
              }
            } catch (_) {
              // Errors are surfaced through provider listener.
            }
          },
          icon: const Icon(Icons.add),
          label: const Text('New Draft'),
        ),
        child: AsyncPage(
          value: page,
          onRetry: () => ref.invalidate(salesPageProvider(campaignId)),
          onRefresh: () => ref.refresh(salesPageProvider(campaignId).future),
          builder: (data) {
            final drafts = data.sales.where((sale) => sale.status.toLowerCase() == 'draft').toList();
            final completed = data.sales
                .where((sale) => sale.status.toLowerCase() == 'completed')
                .toList();
            final voided = data.sales
                .where((sale) => sale.status.toLowerCase() == 'voided')
                .toList();

            return Column(
              children: [
                const TabBar(
                  tabs: [
                    Tab(text: 'Draft'),
                    Tab(text: 'Completed'),
                    Tab(text: 'Void'),
                  ],
                ),
                Expanded(
                  child: TabBarView(
                    children: [
                      _SalesTabList(campaignId: campaignId, rows: drafts, emptyText: 'No drafts.'),
                      _SalesTabList(campaignId: campaignId, rows: completed, emptyText: 'No completed sales.'),
                      _SalesTabList(campaignId: campaignId, rows: voided, emptyText: 'No voided sales.'),
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
    required this.emptyText,
  });

  final String campaignId;
  final List<SalesPageRowDto> rows;
  final String emptyText;

  @override
  Widget build(BuildContext context) {
    if (rows.isEmpty) {
      return Center(child: Text(emptyText));
    }

    return ListView.builder(
      padding: const EdgeInsets.all(16),
      itemCount: rows.length,
      itemBuilder: (context, index) {
        final row = rows[index];
        final isDraft = row.status.toLowerCase() == 'draft';
        return Card(
          margin: const EdgeInsets.only(bottom: 10),
          child: ListTile(
            title: Text('${row.customerName ?? 'Walk-in'} • Day ${row.soldWorldDay}'),
            subtitle: Text('Total: ${row.totalMinor}'),
            trailing: Text(row.status),
            onTap: () => context.go(
              isDraft
                  ? '/campaign/$campaignId/sales/draft/${row.saleId}'
                  : '/campaign/$campaignId/sales/${row.saleId}',
            ),
          ),
        );
      },
    );
  }
}
