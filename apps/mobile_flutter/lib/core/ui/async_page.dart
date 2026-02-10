import 'package:dnd_app/core/errors/app_exception.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class AsyncPage<T> extends StatelessWidget {
  const AsyncPage({
    super.key,
    required this.value,
    required this.builder,
    this.onRetry,
    this.onRefresh,
    this.isEmpty,
    this.emptyMessage = 'No data available.',
  });

  final AsyncValue<T> value;
  final Widget Function(T data) builder;
  final VoidCallback? onRetry;
  final Future<void> Function()? onRefresh;
  final bool Function(T data)? isEmpty;
  final String emptyMessage;

  @override
  Widget build(BuildContext context) {
    return value.when(
      loading: () => const Center(child: CircularProgressIndicator()),
      error: (error, _) => _ErrorState(
        message: error is AppException ? error.message : 'Something went wrong.',
        onRetry: onRetry,
      ),
      data: (data) {
        if (isEmpty != null && isEmpty!(data)) {
          return _EmptyState(message: emptyMessage);
        }

        final child = builder(data);
        if (onRefresh == null) {
          return child;
        }

        return RefreshIndicator(
          onRefresh: onRefresh!,
          child: child,
        );
      },
    );
  }
}

class _ErrorState extends StatelessWidget {
  const _ErrorState({
    required this.message,
    this.onRetry,
  });

  final String message;
  final VoidCallback? onRetry;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            const Icon(Icons.error_outline, size: 34, color: Color(0xFFB64926)),
            const SizedBox(height: 10),
            Text(
              message,
              style: Theme.of(context).textTheme.bodyMedium,
              textAlign: TextAlign.center,
            ),
            const SizedBox(height: 14),
            if (onRetry != null)
              FilledButton.tonal(
                onPressed: onRetry,
                child: const Text('Retry'),
              ),
          ],
        ),
      ),
    );
  }
}

class _EmptyState extends StatelessWidget {
  const _EmptyState({required this.message});

  final String message;

  @override
  Widget build(BuildContext context) {
    return Center(
      child: Padding(
        padding: const EdgeInsets.all(24),
        child: Text(
          message,
          style: Theme.of(context).textTheme.bodyMedium?.copyWith(color: const Color(0xFF516074)),
          textAlign: TextAlign.center,
        ),
      ),
    );
  }
}
