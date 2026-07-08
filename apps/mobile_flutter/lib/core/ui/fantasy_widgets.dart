import 'dart:ui';

import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

final class FantasyColors {
  const FantasyColors._();

  static const ink = Color(0xFF030C10);
  static const deep = Color(0xFF06151A);
  static const panel = Color(0xDD07171D);
  static const panelLight = Color(0xFF0B2228);
  static const border = Color(0xFF1B3E45);
  static const teal = Color(0xFF52F0D0);
  static const tealDim = Color(0xFF008B78);
  static const parchment = Color(0xFFF4F1EA);
  static const muted = Color(0xFFB7C3C1);
}

final class FantasyAssets {
  const FantasyAssets._();

  static const backgroundMap = 'assets/images/background_map.png';
  static const backgroundTurtle = 'assets/images/background_turtle.png';
  static const backgroundTurtleAlt = 'assets/images/background_turtl2.png';
  static const chestStuff = 'assets/images/chest_stuff.png';
  static const paperQuill = 'assets/images/paper_quill.png';
}

class FantasyBackground extends StatelessWidget {
  const FantasyBackground({
    super.key,
    required this.child,
    this.showLandscape = true,
    this.assetPath = FantasyAssets.backgroundMap,
    this.alignment = Alignment.center,
  });

  final Widget child;
  final bool showLandscape;
  final String assetPath;
  final Alignment alignment;

  @override
  Widget build(BuildContext context) {
    return ColoredBox(
      color: FantasyColors.ink,
      child: Stack(
        fit: StackFit.expand,
        children: [
          _BlurredAssetImage(
            assetPath,
            fit: BoxFit.cover,
            alignment: alignment,
            sigma: 1.8,
          ),
          DecoratedBox(
            decoration: BoxDecoration(
              gradient: LinearGradient(
                begin: Alignment.topCenter,
                end: Alignment.bottomCenter,
                colors: [
                  FantasyColors.ink.withValues(alpha: 0.42),
                  FantasyColors.ink.withValues(
                    alpha: showLandscape ? 0.30 : 0.48,
                  ),
                  FantasyColors.ink.withValues(alpha: 0.72),
                ],
              ),
            ),
          ),
          child,
        ],
      ),
    );
  }
}

class FantasyPanel extends StatelessWidget {
  const FantasyPanel({
    super.key,
    required this.child,
    this.padding = const EdgeInsets.all(18),
    this.isHighlighted = false,
  });

  final Widget child;
  final EdgeInsets padding;
  final bool isHighlighted;

  @override
  Widget build(BuildContext context) {
    return DecoratedBox(
      decoration: BoxDecoration(
        borderRadius: BorderRadius.circular(18),
        boxShadow: [
          BoxShadow(
            color: (isHighlighted ? FantasyColors.tealDim : Colors.black)
                .withValues(alpha: isHighlighted ? 0.16 : 0.24),
            blurRadius: isHighlighted ? 16 : 18,
            offset: const Offset(0, 12),
          ),
        ],
      ),
      child: ClipRRect(
        borderRadius: BorderRadius.circular(18),
        child: DecoratedBox(
          decoration: BoxDecoration(
            gradient: LinearGradient(
              colors: [
                isHighlighted ? const Color(0xEE08252A) : FantasyColors.panel,
                isHighlighted
                    ? const Color(0xEE0B2C30)
                    : FantasyColors.panelLight,
              ],
            ),
            border: Border.all(
              color: isHighlighted
                  ? FantasyColors.tealDim
                  : FantasyColors.border,
              width: isHighlighted ? 1.2 : 1,
            ),
            borderRadius: BorderRadius.circular(18),
          ),
          child: Padding(padding: padding, child: child),
        ),
      ),
    );
  }
}

class FantasyNavTile extends StatelessWidget {
  const FantasyNavTile({
    super.key,
    required this.icon,
    required this.label,
    required this.onTap,
    this.isHighlighted = false,
  });

  final IconData icon;
  final String label;
  final VoidCallback? onTap;
  final bool isHighlighted;

  @override
  Widget build(BuildContext context) {
    return Material(
      color: Colors.transparent,
      child: InkWell(
        borderRadius: BorderRadius.circular(18),
        highlightColor: FantasyColors.teal.withValues(alpha: 0.04),
        splashColor: FantasyColors.teal.withValues(alpha: 0.06),
        onTap: onTap,
        child: FantasyPanel(
          isHighlighted: isHighlighted,
          padding: const EdgeInsets.symmetric(horizontal: 18, vertical: 15),
          child: Row(
            children: [
              Icon(
                icon,
                color: isHighlighted
                    ? FantasyColors.parchment
                    : FantasyColors.muted,
                size: 25,
              ),
              const SizedBox(width: 22),
              Expanded(
                child: Text(
                  label,
                  style: GoogleFonts.dmSerifDisplay(
                    color: FantasyColors.parchment,
                    fontSize: 25,
                    fontWeight: FontWeight.w700,
                  ),
                ),
              ),
              Icon(
                Icons.chevron_right,
                color: isHighlighted
                    ? FantasyColors.parchment
                    : FantasyColors.muted,
                size: 25,
              ),
            ],
          ),
        ),
      ),
    );
  }
}

class CampaignHeroCard extends StatelessWidget {
  const CampaignHeroCard({
    super.key,
    required this.title,
    required this.description,
    required this.date,
    required this.role,
  });

  final String title;
  final String? description;
  final Widget date;
  final String role;

  @override
  Widget build(BuildContext context) {
    return SizedBox(
      height: 310,
      child: ClipRRect(
        borderRadius: BorderRadius.circular(18),
        child: Stack(
          fit: StackFit.expand,
          children: [
            _BlurredAssetImage(
              FantasyAssets.backgroundTurtle,
              fit: BoxFit.cover,
              alignment: Alignment.bottomCenter,
              sigma: 1.1,
            ),
            DecoratedBox(
              decoration: BoxDecoration(
                gradient: LinearGradient(
                  begin: Alignment.centerLeft,
                  end: Alignment.centerRight,
                  colors: [
                    Colors.black.withValues(alpha: 0.68),
                    Colors.black.withValues(alpha: 0.18),
                  ],
                ),
                border: Border.all(color: FantasyColors.border),
                borderRadius: BorderRadius.circular(18),
              ),
            ),
            Padding(
              padding: const EdgeInsets.all(26),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    title,
                    style: GoogleFonts.dmSerifDisplay(
                      color: FantasyColors.parchment,
                      fontSize: 38,
                      fontWeight: FontWeight.w800,
                      height: 1,
                    ),
                  ),
                  if ((description ?? '').isNotEmpty) ...[
                    const SizedBox(height: 10),
                    Text(
                      description!,
                      style: const TextStyle(
                        color: FantasyColors.parchment,
                        fontSize: 18,
                        fontWeight: FontWeight.w500,
                      ),
                    ),
                  ],
                  const SizedBox(height: 22),
                  DefaultTextStyle(
                    style: const TextStyle(
                      color: FantasyColors.parchment,
                      fontSize: 17,
                      height: 1.35,
                    ),
                    child: date,
                  ),
                  const Spacer(),
                  Align(
                    alignment: Alignment.bottomLeft,
                    child: DecoratedBox(
                      decoration: BoxDecoration(
                        color: const Color(0xCC092A2E),
                        borderRadius: BorderRadius.circular(28),
                        border: Border.all(color: FantasyColors.tealDim),
                      ),
                      child: Padding(
                        padding: const EdgeInsets.symmetric(
                          horizontal: 10,
                          vertical: 8,
                        ),
                        child: Row(
                          mainAxisSize: MainAxisSize.min,
                          children: [
                            const Icon(
                              Icons.person,
                              color: FantasyColors.teal,
                              size: 12,
                            ),
                            const SizedBox(width: 4),
                            Text(
                              'Role: $role',
                              style: const TextStyle(
                                color: FantasyColors.teal,
                                fontWeight: FontWeight.w700,
                                fontSize: 12,
                              ),
                            ),
                          ],
                        ),
                      ),
                    ),
                  ),
                ],
              ),
            ),
          ],
        ),
      ),
    );
  }
}

class FantasyEmptyState extends StatelessWidget {
  const FantasyEmptyState({
    super.key,
    required this.title,
    required this.message,
    this.variant = FantasyEmptyVariant.chest,
  });

  final String title;
  final String message;
  final FantasyEmptyVariant variant;

  @override
  Widget build(BuildContext context) {
    final assetPath = switch (variant) {
      FantasyEmptyVariant.chest => FantasyAssets.chestStuff,
      FantasyEmptyVariant.scroll => FantasyAssets.paperQuill,
    };

    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 28, vertical: 56),
      child: Column(
        mainAxisAlignment: MainAxisAlignment.center,
        children: [
          SizedBox(
            width: 230,
            height: 185,
            child: Stack(
              alignment: Alignment.center,
              children: [
                Transform.translate(
                  offset: const Offset(0, 10),
                  child: ImageFiltered(
                    imageFilter: ImageFilter.blur(sigmaX: 12, sigmaY: 12),
                    child: Opacity(
                      opacity: 0.48,
                      child: ColorFiltered(
                        colorFilter: const ColorFilter.mode(
                          Color(0xFF001F1C),
                          BlendMode.srcATop,
                        ),
                        child: Image.asset(
                          assetPath,
                          fit: BoxFit.contain,
                          filterQuality: FilterQuality.high,
                        ),
                      ),
                    ),
                  ),
                ),
                Image.asset(
                  assetPath,
                  fit: BoxFit.contain,
                  filterQuality: FilterQuality.high,
                ),
              ],
            ),
          ),
          const SizedBox(height: 22),
          Text(
            title,
            textAlign: TextAlign.center,
            style: GoogleFonts.dmSerifDisplay(
              color: FantasyColors.parchment,
              fontSize: 29,
              fontWeight: FontWeight.w800,
            ),
          ),
          const SizedBox(height: 12),
          ConstrainedBox(
            constraints: const BoxConstraints(maxWidth: 280),
            child: Text(
              message,
              textAlign: TextAlign.center,
              style: const TextStyle(
                color: FantasyColors.muted,
                fontSize: 16,
                height: 1.45,
              ),
            ),
          ),
        ],
      ),
    );
  }
}

enum FantasyEmptyVariant { chest, scroll }

class _BlurredAssetImage extends StatelessWidget {
  const _BlurredAssetImage(
    this.assetPath, {
    required this.fit,
    required this.alignment,
    required this.sigma,
  });

  final String assetPath;
  final BoxFit fit;
  final Alignment alignment;
  final double sigma;

  @override
  Widget build(BuildContext context) {
    return ClipRect(
      child: ImageFiltered(
        imageFilter: ImageFilter.blur(sigmaX: sigma, sigmaY: sigma),
        child: Image.asset(
          assetPath,
          fit: fit,
          alignment: alignment,
          filterQuality: FilterQuality.high,
        ),
      ),
    );
  }
}

class FantasyDivider extends StatelessWidget {
  const FantasyDivider({super.key});

  @override
  Widget build(BuildContext context) {
    return Row(
      children: [
        const Expanded(child: Divider(color: FantasyColors.border)),
        Padding(
          padding: const EdgeInsets.symmetric(horizontal: 12),
          child: Icon(
            Icons.casino_outlined,
            color: FantasyColors.teal.withValues(alpha: 0.75),
            size: 25,
          ),
        ),
        const Expanded(child: Divider(color: FantasyColors.border)),
      ],
    );
  }
}
