import 'package:dnd_app/core/api/models/common_models.dart';

List<CurrencyDenominationDto> sortedDenominations(CurrencyConfigDto currency) {
  return [...currency.denominations]..sort((a, b) {
    final byMultiplier = b.multiplier.compareTo(a.multiplier);
    return byMultiplier != 0 ? byMultiplier : a.name.compareTo(b.name);
  });
}

CurrencyConfigDto currencyWithDndCoinFallbacks(CurrencyConfigDto currency) {
  const dndMultipliers = {
    'copper': 1,
    'silver': 10,
    'gold': 100,
    'platinum': 1000,
  };

  final seenNames = <String>{};
  final denominations = <CurrencyDenominationDto>[];

  for (final denomination in currency.denominations) {
    final normalizedName = denomination.name.toLowerCase();
    seenNames.add(normalizedName);
    denominations.add(
      CurrencyDenominationDto(
        name: denomination.name,
        multiplier: dndMultipliers[normalizedName] ?? denomination.multiplier,
      ),
    );
  }

  for (final entry in dndMultipliers.entries) {
    if (!seenNames.contains(entry.key)) {
      denominations.add(
        CurrencyDenominationDto(name: entry.key, multiplier: entry.value),
      );
    }
  }

  return CurrencyConfigDto(
    campaignId: currency.campaignId,
    currencyCode: currency.currencyCode,
    minorUnitName: currency.minorUnitName,
    majorUnitName: currency.majorUnitName,
    denominations: denominations,
  );
}

Map<String, int> coinCountsFromMinor(
  int amountMinor,
  CurrencyConfigDto currency,
) {
  final sign = amountMinor < 0 ? -1 : 1;
  final suggested = suggestCoinCounts(amountMinor.abs(), currency);
  return {for (final entry in suggested.entries) entry.key: entry.value * sign};
}

int coinCountsTotalMinor(
  Map<String, int> counts,
  List<CurrencyDenominationDto> denominations,
) {
  var total = 0;
  for (final denomination in denominations) {
    final quantity = counts[denomination.name] ?? 0;
    total += denomination.multiplier * quantity;
  }

  return total;
}

Map<String, int> suggestCoinCounts(
  int amountMinor,
  CurrencyConfigDto currency,
) {
  var remaining = amountMinor;
  final result = <String, int>{};
  for (final denomination in sortedDenominations(currency)) {
    if (denomination.multiplier <= 0) {
      continue;
    }

    final quantity = remaining ~/ denomination.multiplier;
    result[denomination.name] = quantity;
    remaining -= quantity * denomination.multiplier;
  }

  return result;
}

Map<String, int> suggestTenderedCoinCounts(
  int amountMinor,
  CurrencyConfigDto currency,
) {
  var remaining = amountMinor;
  final result = {
    for (final denomination in currency.denominations) denomination.name: 0,
  };
  final denominations = sortedDenominations(currency)
      .where((denomination) => denomination.name.toLowerCase() != 'platinum')
      .toList();

  for (final denomination in denominations) {
    if (denomination.multiplier <= 0) {
      continue;
    }

    final quantity = remaining ~/ denomination.multiplier;
    result[denomination.name] = quantity;
    remaining -= quantity * denomination.multiplier;
  }

  return result;
}

Map<String, dynamic> buildCoinPaymentDetails(
  CurrencyConfigDto currency,
  Map<String, int> counts,
) {
  final coins = sortedDenominations(currency)
      .map(
        (denomination) => {
          'name': denomination.name,
          'multiplier': denomination.multiplier,
          'quantity': counts[denomination.name] ?? 0,
        },
      )
      .where((coin) => (coin['quantity'] as int) > 0)
      .toList();

  return {
    'type': 'coin-denominations',
    'currencyCode': currency.currencyCode,
    'coins': coins,
  };
}

String formatCoinPaymentDetails(Object? details) {
  if (details is! Map) {
    return '';
  }

  final type = details['type']?.toString();
  if (type?.toLowerCase() != 'coin-denominations') {
    return '';
  }

  final coins = details['coins'];
  if (coins is! List) {
    return '';
  }

  final parts = <_CoinPart>[];
  for (final coin in coins) {
    if (coin is! Map) {
      continue;
    }

    final name = coin['name']?.toString() ?? '';
    final multiplier = (coin['multiplier'] as num?)?.toInt() ?? 0;
    final quantity = (coin['quantity'] as num?)?.toInt() ?? 0;
    if (name.isEmpty || multiplier <= 0 || quantity <= 0) {
      continue;
    }

    parts.add(
      _CoinPart(name: name, multiplier: multiplier, quantity: quantity),
    );
  }

  parts.sort((a, b) => b.multiplier.compareTo(a.multiplier));
  return parts.map((part) => '${part.quantity} ${part.name}').join(' ');
}

class _CoinPart {
  const _CoinPart({
    required this.name,
    required this.multiplier,
    required this.quantity,
  });

  final String name;
  final int multiplier;
  final int quantity;
}
