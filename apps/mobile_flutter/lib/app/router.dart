import 'dart:async';

import 'package:dnd_app/core/auth/session_controller.dart';
import 'package:dnd_app/features/auth/invite_signup_page.dart';
import 'package:dnd_app/features/auth/login_page.dart';
import 'package:dnd_app/features/campaigns/campaign_home_page.dart';
import 'package:dnd_app/features/campaigns/campaign_select_page.dart';
import 'package:dnd_app/features/catalog/catalog_item_detail_page.dart';
import 'package:dnd_app/features/catalog/catalog_list_page.dart';
import 'package:dnd_app/features/inventory/inventory_location_detail_page.dart';
import 'package:dnd_app/features/inventory/inventory_locations_page.dart';
import 'package:dnd_app/features/inventory/inventory_summary_page.dart';
import 'package:dnd_app/features/sales/draft_sale_page.dart';
import 'package:dnd_app/features/sales/sale_detail_page.dart';
import 'package:dnd_app/features/sales/sales_list_page.dart';
import 'package:dnd_app/features/settings/settings_page.dart';
import 'package:dnd_app/features/setup/server_setup_page.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

final routerProvider = Provider<GoRouter>((ref) {
  final sessionNotifier = ref.read(sessionControllerProvider.notifier);
  final refreshListenable = _RouterRefreshListenable(sessionNotifier.stream);
  ref.onDispose(refreshListenable.dispose);

  return GoRouter(
    initialLocation: '/splash',
    refreshListenable: refreshListenable,
    redirect: (context, state) {
      final session = ref.read(sessionControllerProvider);
      final location = state.matchedLocation;
      final isSplash = location == '/splash';
      final isSetup = location == '/setup';
      final isLoginRoute = location == '/login' || location == '/signup';

      if (!session.initialized) {
        return isSplash ? null : '/splash';
      }

      if (session.baseUrl == null || session.baseUrl!.isEmpty) {
        return isSetup ? null : '/setup';
      }

      if (!session.isAuthenticated) {
        return isLoginRoute ? null : '/login';
      }

      if (isSplash || isSetup || isLoginRoute) {
        final selectedCampaignId = session.selectedCampaignId;
        if (selectedCampaignId == null || selectedCampaignId.isEmpty) {
          return '/campaigns';
        }

        return '/campaign/$selectedCampaignId/home';
      }

      return null;
    },
    routes: [
      GoRoute(
        path: '/splash',
        builder: (context, state) => const _SplashPage(),
      ),
      GoRoute(
        path: '/setup',
        builder: (context, state) => const ServerSetupPage(),
      ),
      GoRoute(
        path: '/login',
        builder: (context, state) => const LoginPage(),
      ),
      GoRoute(
        path: '/signup',
        builder: (context, state) => const InviteSignupPage(),
      ),
      GoRoute(
        path: '/campaigns',
        builder: (context, state) => const CampaignSelectPage(),
      ),
      GoRoute(
        path: '/campaign/:campaignId/home',
        builder: (context, state) => CampaignHomePage(
          campaignId: state.pathParameters['campaignId']!,
        ),
      ),
      GoRoute(
        path: '/campaign/:campaignId/catalog',
        builder: (context, state) => CatalogListPage(
          campaignId: state.pathParameters['campaignId']!,
        ),
      ),
      GoRoute(
        path: '/campaign/:campaignId/catalog/item/:itemId',
        builder: (context, state) => CatalogItemDetailPage(
          campaignId: state.pathParameters['campaignId']!,
          itemId: state.pathParameters['itemId']!,
        ),
      ),
      GoRoute(
        path: '/campaign/:campaignId/inventory',
        builder: (context, state) => InventorySummaryPage(
          campaignId: state.pathParameters['campaignId']!,
        ),
      ),
      GoRoute(
        path: '/campaign/:campaignId/inventory/locations',
        builder: (context, state) => InventoryLocationsPage(
          campaignId: state.pathParameters['campaignId']!,
        ),
      ),
      GoRoute(
        path: '/campaign/:campaignId/inventory/location/:locationId',
        builder: (context, state) => InventoryLocationDetailPage(
          campaignId: state.pathParameters['campaignId']!,
          locationId: state.pathParameters['locationId']!,
        ),
      ),
      GoRoute(
        path: '/campaign/:campaignId/sales',
        builder: (context, state) => SalesListPage(
          campaignId: state.pathParameters['campaignId']!,
        ),
      ),
      GoRoute(
        path: '/campaign/:campaignId/sales/draft/:draftId',
        builder: (context, state) => DraftSalePage(
          campaignId: state.pathParameters['campaignId']!,
          draftId: state.pathParameters['draftId']!,
        ),
      ),
      GoRoute(
        path: '/campaign/:campaignId/sales/:saleId',
        builder: (context, state) => SaleDetailPage(
          campaignId: state.pathParameters['campaignId']!,
          saleId: state.pathParameters['saleId']!,
        ),
      ),
      GoRoute(
        path: '/campaign/:campaignId/settings',
        builder: (context, state) => SettingsPage(
          campaignId: state.pathParameters['campaignId']!,
        ),
      ),
    ],
  );
});

class _SplashPage extends StatelessWidget {
  const _SplashPage();

  @override
  Widget build(BuildContext context) {
    return const Scaffold(
      body: Center(child: CircularProgressIndicator()),
    );
  }
}

class _RouterRefreshListenable extends ChangeNotifier {
  _RouterRefreshListenable(Stream<dynamic> stream) {
    _subscription = stream.listen((_) => notifyListeners());
  }

  late final StreamSubscription<dynamic> _subscription;

  @override
  void dispose() {
    _subscription.cancel();
    super.dispose();
  }
}
