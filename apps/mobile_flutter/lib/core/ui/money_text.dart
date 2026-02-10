import 'package:dnd_app/core/api/models/common_models.dart';
import 'package:dnd_app/core/utils/money_formatter.dart';
import 'package:flutter/material.dart';

class MoneyText extends StatelessWidget {
  const MoneyText({
    super.key,
    required this.amountMinor,
    required this.currency,
    this.style,
  });

  final int amountMinor;
  final CurrencyConfigDto currency;
  final TextStyle? style;

  @override
  Widget build(BuildContext context) {
    return Text(
      formatMoneyMinorUnits(amountMinor, currency),
      style: style,
    );
  }
}
