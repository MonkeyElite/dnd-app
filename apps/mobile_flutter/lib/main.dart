import 'package:flutter/material.dart';

void main() {
  runApp(const DndApp());
}

class DndApp extends StatelessWidget {
  const DndApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'D&D App',
      theme: ThemeData(
        colorScheme: ColorScheme.fromSeed(seedColor: Colors.red),
      ),
      home: const HomePage(),
    );
  }
}

class HomePage extends StatelessWidget {
  const HomePage({super.key});

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('D&D App')),
      body: const Center(
        child: Text('D&D App'),
      ),
    );
  }
}
