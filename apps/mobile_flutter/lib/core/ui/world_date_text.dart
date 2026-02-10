import 'package:dnd_app/core/api/models/common_models.dart';
import 'package:dnd_app/core/utils/world_date_formatter.dart';
import 'package:flutter/material.dart';

class WorldDateText extends StatelessWidget {
  const WorldDateText({
    super.key,
    required this.worldDay,
    required this.calendar,
    this.style,
  });

  final int worldDay;
  final CalendarConfigDto calendar;
  final TextStyle? style;

  @override
  Widget build(BuildContext context) {
    return Text(
      formatWorldDate(worldDay: worldDay, calendar: calendar),
      style: style,
    );
  }
}
