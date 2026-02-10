import 'package:dio/dio.dart';
import 'package:dnd_app/core/auth/session_controller.dart';
import 'package:dnd_app/core/ui/ui.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';

class ServerSetupPage extends ConsumerStatefulWidget {
  const ServerSetupPage({super.key});

  @override
  ConsumerState<ServerSetupPage> createState() => _ServerSetupPageState();
}

class _ServerSetupPageState extends ConsumerState<ServerSetupPage> {
  final _formKey = GlobalKey<FormState>();
  late final TextEditingController _baseUrlController;
  bool _isTesting = false;
  bool _isSaving = false;

  @override
  void initState() {
    super.initState();
    final existingBaseUrl = ref.read(sessionControllerProvider).baseUrl;
    _baseUrlController = TextEditingController(text: existingBaseUrl ?? 'http://10.0.2.2:7000');
  }

  @override
  void dispose() {
    _baseUrlController.dispose();
    super.dispose();
  }

  Future<void> _testConnection() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    setState(() => _isTesting = true);
    final url = _baseUrlController.text.trim().replaceAll(RegExp(r'/$'), '');

    try {
      final dio = Dio(BaseOptions(baseUrl: url, connectTimeout: const Duration(seconds: 8)));
      final response = await dio.get('/api/v1/health');

      if (!mounted) {
        return;
      }

      final ok = response.statusCode == 200;
      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(
          content: Text(ok ? 'Connection successful.' : 'Unexpected status: ${response.statusCode}'),
        ),
      );
    } on DioException catch (error) {
      if (!mounted) {
        return;
      }

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Connection failed: ${error.message ?? 'Unknown error'}')),
      );
    } finally {
      if (mounted) {
        setState(() => _isTesting = false);
      }
    }
  }

  Future<void> _save() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    setState(() => _isSaving = true);
    final url = _baseUrlController.text.trim().replaceAll(RegExp(r'/$'), '');
    await ref.read(sessionControllerProvider.notifier).setBaseUrl(url);

    if (!mounted) {
      return;
    }

    setState(() => _isSaving = false);
    context.go('/login');
  }

  @override
  Widget build(BuildContext context) {
    return AppScaffold(
      title: 'Server Setup',
      child: ListView(
        padding: const EdgeInsets.all(20),
        children: [
          InfoCard(
            child: Form(
              key: _formKey,
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    'Connect to your BFF server',
                    style: Theme.of(context).textTheme.titleMedium,
                  ),
                  const SizedBox(height: 12),
                  RoundedTextField(
                    controller: _baseUrlController,
                    label: 'Base URL',
                    hintText: 'http://10.0.2.2:7000',
                    keyboardType: TextInputType.url,
                    prefixIcon: Icons.link,
                    validator: (value) {
                      final text = value?.trim() ?? '';
                      if (text.isEmpty) {
                        return 'Base URL is required.';
                      }

                      final uri = Uri.tryParse(text);
                      if (uri == null || !uri.isAbsolute) {
                        return 'Enter a valid absolute URL.';
                      }

                      return null;
                    },
                  ),
                  const SizedBox(height: 16),
                  SecondaryButton(
                    label: _isTesting ? 'Testing...' : 'Test Connection',
                    onPressed: _isTesting ? null : _testConnection,
                  ),
                  const SizedBox(height: 10),
                  PrimaryPillButton(
                    label: 'Save',
                    onPressed: _isSaving ? null : _save,
                    isLoading: _isSaving,
                  ),
                ],
              ),
            ),
          ),
        ],
      ),
    );
  }
}
