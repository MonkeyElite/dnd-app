import 'package:flutter/material.dart';

import 'fantasy_widgets.dart';

class AppScaffold extends StatelessWidget {
  const AppScaffold({
    super.key,
    required this.title,
    required this.child,
    this.actions,
    this.floatingActionButton,
    this.backgroundBuilder,
    this.backgroundAsset,
    this.backgroundAlignment = Alignment.center,
  });

  final String title;
  final Widget child;
  final List<Widget>? actions;
  final Widget? floatingActionButton;
  final Widget Function(BuildContext context, Widget content)?
  backgroundBuilder;
  final String? backgroundAsset;
  final Alignment backgroundAlignment;

  @override
  Widget build(BuildContext context) {
    final content = SafeArea(
      child: Stack(children: [Positioned.fill(child: child)]),
    );

    return Scaffold(
      appBar: AppBar(title: Text(title), actions: _spacedActions(actions)),
      floatingActionButton: floatingActionButton,
      body: backgroundBuilder != null
          ? backgroundBuilder!(context, content)
          : _defaultBackground(content),
    );
  }

  Widget _defaultBackground(Widget content) {
    return FantasyBackground(
      assetPath: backgroundAsset ?? FantasyAssets.backgroundMap,
      alignment: backgroundAlignment,
      child: content,
    );
  }

  List<Widget>? _spacedActions(List<Widget>? actions) {
    if (actions == null || actions.isEmpty) {
      return actions;
    }

    return [
      for (var index = 0; index < actions.length; index++)
        Padding(
          padding: EdgeInsets.only(right: index == actions.length - 1 ? 14 : 8),
          child: actions[index],
        ),
    ];
  }
}
