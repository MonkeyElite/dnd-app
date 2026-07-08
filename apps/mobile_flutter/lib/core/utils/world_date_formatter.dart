import 'package:dnd_app/core/api/models/common_models.dart';

String formatWorldDate({
  required int worldDay,
  required CalendarConfigDto calendar,
}) {
  if (calendar.months.isEmpty || worldDay < 0) {
    return 'Day $worldDay';
  }

  final totalDaysInYear = calendar.months.fold<int>(
    0,
    (sum, month) => sum + month.days,
  );
  if (totalDaysInYear <= 0) {
    return 'Day $worldDay';
  }

  final year = (worldDay ~/ totalDaysInYear) + 1;
  final dayOfYear = worldDay % totalDaysInYear;
  var remaining = dayOfYear;
  for (final month in calendar.months) {
    if (remaining < month.days) {
      final dayOfMonth = remaining + 1;
      final weekDay = calendar.weekLength <= 0
          ? 1
          : (worldDay % calendar.weekLength) + 1;
      return 'Year $year, ${month.name} $dayOfMonth, week day $weekDay';
    }

    remaining -= month.days;
  }
  return 'Day $worldDay';
}
