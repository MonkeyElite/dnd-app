import 'package:flutter/material.dart';

import 'fantasy_widgets.dart';

class InfoCard extends StatelessWidget {
  const InfoCard({
    super.key,
    required this.child,
    this.padding = const EdgeInsets.all(16),
  });

  final Widget child;
  final EdgeInsets padding;

  @override
  Widget build(BuildContext context) {
    return FantasyPanel(
      padding: padding,
      child: child,
    );
  }
}
