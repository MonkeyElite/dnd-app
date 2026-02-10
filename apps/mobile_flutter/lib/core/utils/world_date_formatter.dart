import 'package:dnd_app/core/api/models/common_models.dart';

String formatWorldDate({
  required int worldDay,
  required CalendarConfigDto calendar,
}) {
  if (calendar.months.isEmpty || worldDay < 0) {
    return 'Day $worldDay';
  }

  var remaining = worldDay;
  for (final month in calendar.months) {
    if (remaining < month.days) {
      final dayOfMonth = remaining + 1;
      final weekDay = (worldDay % calendar.weekLength) + 1;
      return '${month.name} $dayOfMonth, week day $weekDay';
    }

    remaining -= month.days;
  }

  final totalDaysInYear = calendar.months.fold<int>(0, (sum, month) => sum + month.days);
  if (totalDaysInYear <= 0) {
    return 'Day $worldDay';
  }

  final wrappedDay = worldDay % totalDaysInYear;
  return formatWorldDate(worldDay: wrappedDay, calendar: calendar);
}
