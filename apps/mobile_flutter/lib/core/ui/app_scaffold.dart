import 'package:flutter/material.dart';

class AppScaffold extends StatelessWidget {
  const AppScaffold({
    super.key,
    required this.title,
    required this.child,
    this.actions,
    this.floatingActionButton,
    this.backgroundBuilder,
  });

  final String title;
  final Widget child;
  final List<Widget>? actions;
  final Widget? floatingActionButton;
  final Widget Function(BuildContext context, Widget content)? backgroundBuilder;

  @override
  Widget build(BuildContext context) {
    final content = SafeArea(
      child: Stack(
        children: [
          Positioned(
            top: 6,
            right: 8,
            child: Icon(
              Icons.shield_outlined,
              color: Theme.of(context).colorScheme.primary.withValues(alpha: 0.18),
              size: 22,
            ),
          ),
          Positioned.fill(child: child),
        ],
      ),
    );

    return Scaffold(
      appBar: AppBar(
        title: Text(title),
        actions: actions,
      ),
      floatingActionButton: floatingActionButton,
      body: backgroundBuilder != null ? backgroundBuilder!(context, content) : _defaultBackground(content),
    );
  }

  Widget _defaultBackground(Widget content) {
    return Container(
      decoration: const BoxDecoration(
        gradient: LinearGradient(
          begin: Alignment.topCenter,
          end: Alignment.bottomCenter,
          colors: [Color(0xFFF0F7FF), Color(0xFFF8FBFF)],
        ),
      ),
      child: content,
    );
  }
}
