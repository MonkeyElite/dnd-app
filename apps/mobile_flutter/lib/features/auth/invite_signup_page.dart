import 'package:dnd_app/core/errors/app_exception.dart';
import 'package:dnd_app/core/ui/ui.dart';
import 'package:dnd_app/features/auth/auth_controller.dart';
import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

class InviteSignupPage extends ConsumerStatefulWidget {
  const InviteSignupPage({super.key});

  @override
  ConsumerState<InviteSignupPage> createState() => _InviteSignupPageState();
}

class _InviteSignupPageState extends ConsumerState<InviteSignupPage> {
  final _formKey = GlobalKey<FormState>();
  final _inviteCodeController = TextEditingController();
  final _usernameController = TextEditingController();
  final _displayNameController = TextEditingController();
  final _passwordController = TextEditingController();

  @override
  void dispose() {
    _inviteCodeController.dispose();
    _usernameController.dispose();
    _displayNameController.dispose();
    _passwordController.dispose();
    super.dispose();
  }

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) {
      return;
    }

    await ref.read(authControllerProvider.notifier).registerInvite(
          inviteCode: _inviteCodeController.text,
          username: _usernameController.text,
          displayName: _displayNameController.text,
          password: _passwordController.text,
        );
  }

  @override
  Widget build(BuildContext context) {
    ref.listen<AsyncValue<void>>(authControllerProvider, (previous, next) {
      next.whenOrNull(
        error: (error, _) {
          final message = error is AppException ? error.message : 'Invite sign-up failed.';
          ScaffoldMessenger.of(context).showSnackBar(SnackBar(content: Text(message)));
        },
      );
    });

    final state = ref.watch(authControllerProvider);

    return AppScaffold(
      title: 'Invite Sign-Up',
      child: ListView(
        padding: const EdgeInsets.all(20),
        children: [
          InfoCard(
            child: Form(
              key: _formKey,
              child: Column(
                children: [
                  RoundedTextField(
                    controller: _inviteCodeController,
                    label: 'Invite Code',
                    validator: (value) =>
                        (value == null || value.trim().isEmpty) ? 'Invite code is required.' : null,
                  ),
                  const SizedBox(height: 10),
                  RoundedTextField(
                    controller: _usernameController,
                    label: 'Username',
                    validator: (value) =>
                        (value == null || value.trim().isEmpty) ? 'Username is required.' : null,
                  ),
                  const SizedBox(height: 10),
                  RoundedTextField(
                    controller: _displayNameController,
                    label: 'Display Name',
                    validator: (value) => (value == null || value.trim().isEmpty)
                        ? 'Display name is required.'
                        : null,
                  ),
                  const SizedBox(height: 10),
                  RoundedTextField(
                    controller: _passwordController,
                    label: 'Password',
                    obscureText: true,
                    validator: (value) {
                      if (value == null || value.isEmpty) {
                        return 'Password is required.';
                      }
                      if (value.length < 8) {
                        return 'Password must be at least 8 characters.';
                      }
                      return null;
                    },
                  ),
                  const SizedBox(height: 16),
                  PrimaryPillButton(
                    label: 'Create Account',
                    onPressed: state.isLoading ? null : _submit,
                    isLoading: state.isLoading,
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
