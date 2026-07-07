import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';

final class AppTheme {
  const AppTheme._();

  static ThemeData fantasyDark() {
    const seed = Color(0xFF52F0D0);
    const background = Color(0xFF030C10);
    const surface = Color(0xFF07171D);
    const border = Color(0xFF193841);
    final colorScheme = ColorScheme.fromSeed(
      seedColor: seed,
      brightness: Brightness.dark,
      primary: seed,
      secondary: const Color(0xFFB9F7E8),
      surface: surface,
      onSurface: const Color(0xFFF4F1EA),
    );

    final base = ThemeData(
      useMaterial3: true,
      colorScheme: colorScheme,
      scaffoldBackgroundColor: background,
      textTheme: GoogleFonts.interTextTheme(ThemeData.dark().textTheme),
    );

    return base.copyWith(
      appBarTheme: AppBarTheme(
        elevation: 0,
        scrolledUnderElevation: 0,
        backgroundColor: Colors.transparent,
        foregroundColor: const Color(0xFFF4F1EA),
        centerTitle: false,
        titleTextStyle: GoogleFonts.dmSerifDisplay(
          color: const Color(0xFFF4F1EA),
          fontSize: 28,
          fontWeight: FontWeight.w700,
          height: 1.1,
        ),
      ),
      inputDecorationTheme: InputDecorationTheme(
        filled: true,
        fillColor: const Color(0xCC07171D),
        hintStyle: const TextStyle(color: Color(0xFF9AA9AE)),
        labelStyle: const TextStyle(color: Color(0xFFBFCBCB)),
        prefixIconColor: const Color(0xFFC8D4D6),
        suffixIconColor: seed,
        border: OutlineInputBorder(
          borderRadius: BorderRadius.circular(14),
          borderSide: const BorderSide(color: border),
        ),
        enabledBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(14),
          borderSide: const BorderSide(color: border),
        ),
        focusedBorder: OutlineInputBorder(
          borderRadius: BorderRadius.circular(14),
          borderSide: const BorderSide(color: seed, width: 1.4),
        ),
        contentPadding: const EdgeInsets.symmetric(horizontal: 18, vertical: 16),
      ),
      cardTheme: CardThemeData(
        color: const Color(0xDD07171D),
        elevation: 0,
        shadowColor: const Color(0x6600E6C0),
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(18),
          side: const BorderSide(color: border),
        ),
      ),
      elevatedButtonTheme: ElevatedButtonThemeData(
        style: ElevatedButton.styleFrom(
          shape: const StadiumBorder(),
          minimumSize: const Size.fromHeight(48),
          elevation: 0,
          backgroundColor: seed,
          foregroundColor: const Color(0xFF031013),
        ),
      ),
      filledButtonTheme: FilledButtonThemeData(
        style: FilledButton.styleFrom(
          shape: const StadiumBorder(),
          minimumSize: const Size.fromHeight(50),
          backgroundColor: seed,
          foregroundColor: const Color(0xFF031013),
          textStyle: const TextStyle(fontWeight: FontWeight.w800),
        ),
      ),
      outlinedButtonTheme: OutlinedButtonThemeData(
        style: OutlinedButton.styleFrom(
          shape: const StadiumBorder(),
          minimumSize: const Size.fromHeight(46),
          foregroundColor: const Color(0xFFF4F1EA),
          side: const BorderSide(color: border),
        ),
      ),
      textButtonTheme: TextButtonThemeData(
        style: TextButton.styleFrom(
          foregroundColor: seed,
          shape: const StadiumBorder(),
        ),
      ),
      floatingActionButtonTheme: const FloatingActionButtonThemeData(
        backgroundColor: Color(0xFF00A88D),
        foregroundColor: Color(0xFFF4F1EA),
        extendedTextStyle: TextStyle(fontSize: 16, fontWeight: FontWeight.w800),
      ),
      iconButtonTheme: IconButtonThemeData(
        style: IconButton.styleFrom(
          foregroundColor: const Color(0xFFF4F1EA),
          backgroundColor: const Color(0x9907171D),
          side: const BorderSide(color: border),
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
          fixedSize: const Size.square(48),
        ),
      ),
      tabBarTheme: const TabBarThemeData(
        dividerColor: border,
        indicatorColor: seed,
        labelColor: seed,
        unselectedLabelColor: Color(0xFFE7E0D5),
        indicatorSize: TabBarIndicatorSize.tab,
      ),
    );
  }
}
