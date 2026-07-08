import 'package:dnd_app/core/api/models/common_models.dart';
import 'package:dnd_app/core/utils/currency_utils.dart';

String formatMoneyMinorUnits(int amountMinor, CurrencyConfigDto currency) {
  final isNegative = amountMinor < 0;
  var remaining = amountMinor.abs();

  final parts = <String>[];
  for (final denomination in sortedDenominations(currency)) {
    if (denomination.multiplier <= 0) {
      continue;
    }

    final count = remaining ~/ denomination.multiplier;
    if (count > 0) {
      parts.add('$count ${denomination.name}');
      remaining -= count * denomination.multiplier;
    }
  }

  if (parts.isEmpty) {
    parts.add('0 ${currency.minorUnitName}');
  } else if (remaining > 0) {
    parts.add('$remaining ${currency.minorUnitName}');
  }

  final formatted = parts.join(' ');
  return isNegative ? '-$formatted' : formatted;
}
