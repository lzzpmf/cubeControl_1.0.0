# Integrações WPF UI

[Documentação](../README.md) · [Design system](../DESIGN_SYSTEM.md) · Revisão: 01/10/2026

O projeto usa `WPF-UI` **4.3.0**, fixado em [Cube.Desktop.csproj](../../windows/Cube.Desktop/Cube.Desktop.csproj). A antiga lista de propostas misturava componentes já entregues com possibilidades futuras.

## Implementado

| Recurso | Aplicação e limite |
| --- | --- |
| `ApplicationThemeManager` e `SystemThemeWatcher` | Claro/Escuro/Sistema; o watcher de tema acompanha o Windows no modo Sistema |
| Recursos dinâmicos | Superfícies, textos, seleção, foco, knobs, ícones e onda; cores não alteram fotos |
| `SnackbarService` / `SnackbarPresenter` | Resultados breves, conforme [Notificações](snackbars.md) |
| `InfoBar` | Avisos e erros no editor |
| `ProgressRing` / `ProgressBar` | Espera sem total e progresso medido, com apresentação centralizada |
| `ContentDialog` / `MessageBox` | Galerias, associações, confirmações e avisos localizados |
| `CardAction` | Ações e superfícies usando templates da biblioteca |
| Mica | Integrado à preferência de transparência e ao fallback do Windows |

O adaptador `ParameterNumberBox` permanece no código. Isso não significa que os knobs da Pedaleira exibam campos numéricos: esses campos foram removidos em uma revisão posterior. O criador tem seus próprios campos de edição.

## Possibilidades ainda sem integração identificada

| Recurso | Questão para uma futura implementação |
| --- | --- |
| `TaskBarService` | Vincular progresso real à barra de tarefas |
| `ContentDialogService` | Centralizar serviços de diálogo; os diálogos atuais já existem por construção direta/helpers |
| `NavigationService` | Avaliar páginas sem recriar a sessão MIDI |
| Bandeja do sistema | Definir ciclo de vida, visibilidade da conexão e comportamento ao fechar |

Não tratar essas possibilidades como requisitos já aprovados ou funções disponíveis. Temas, Mica e snackbars já implementados não pertencem a essa lista de pendências.

## Referências

Código local: [ThemeService](../../windows/Cube.Desktop/Appearance/ThemeService.cs), [janela](../../windows/Cube.Desktop/MainWindow.xaml.cs), [diálogos localizados](../../windows/Cube.Desktop/Localization/LocalizedDialogs.cs) e [ParameterNumberBox](../../windows/Cube.Desktop/Controls/ParameterNumberBox.cs).

Documentação upstream: [temas](https://wpfui.lepo.co/documentation/themes), [SystemThemeWatcher](https://wpfui.lepo.co/api/Wpf.Ui.Appearance.SystemThemeWatcher.html), [InfoBar](https://wpfui.lepo.co/api/Wpf.Ui.Controls.InfoBar.html), [SnackbarService](https://wpfui.lepo.co/api/Wpf.Ui.SnackbarService.html). As páginas online podem acompanhar versões posteriores; confira o contrato da versão fixada antes de alterar o produto.
