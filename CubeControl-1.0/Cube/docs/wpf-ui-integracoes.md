# WPF UI: temas e próximas integrações

## Entregue na versão 3.2.0

- Claro, Escuro e Sistema no botão Tema, com preferência local independente do idioma e dos presets.
- `ApplicationThemeManager` troca o tema da biblioteca; `SystemThemeWatcher` acompanha o Windows somente no modo Sistema. Ao selecionar Claro/Escuro, a observação é removida.
- Recursos dinâmicos para superfícies, textos, foco, seleção, knobs e forma de onda. Cores dos módulos são mais escuras no tema claro para manter legibilidade; fotos não são alteradas.
- Cor de destaque do Windows nos componentes WPF UI. A cadeia mantém suas cores por módulo.
- `ProgressRing` discreto durante operações do ViewModel, como conexão, leitura, salvamento e transferências. Não representa progresso percentual nem conclusão da verificação de dados.
- Validação direcionada: preferência persistida e inválida, troca de recursos existentes, isolamento dos modos manuais, recebimento de `WM_SETTINGCHANGE`, remoção do watcher e ausência de edição/MIDI durante a troca. Foram revisadas capturas offline das três áreas e dos diálogos em ambos os temas. A configuração global de tema do Windows não foi alterada no ensaio.

## Integrações úteis avaliadas (ainda não implementadas)

| Prioridade | Recurso | Aplicação e critério |
| --- | --- | --- |
| Alta | InfoBar | Erros que exigem ação: perda de conexão, conflito com alterações externas e restauração de prévia não confirmada. Manter o aviso visível até resolver; oferecer a ação existente de reconectar/reler quando apropriado. |
| Alta | Snackbar | Sucesso ao salvar/exportar e criar backup, sem bloquear a edição. Não disparar para cada movimento de knob. Não usar avisos temporários como único relato de aplicação parcial ou erro. |
| Média | NumberBox | Substituir campos numéricos por entrada com limites e botões incrementais. Antes de adotar, verificar Enter/perda de foco, unidades reais e agrupamento de desfazer; não enviar texto incompleto ao pedal. |
| Média | TaskBarService | Exibir progresso de operações longas na barra de tarefas. Ligar ao progresso real da transferência/verificação; operações sem total conhecido ficam indeterminadas. |
| Média | ContentDialogService | Centralizar os diálogos já existentes e suas traduções. Conservar confirmações explícitas de sobrescrita/restauração e criar backups antes delas. |
| Baixa | NavigationService | Separar as três áreas em páginas quando crescerem. Preservar a mesma sessão MIDI e fila; navegar nunca deve recriar a conexão. |
| Opcional | Mica/Acrylic | Avaliar como preferência estética futura. Superfícies opacas atuais mantêm previsibilidade de contraste; o tema não depende de transparência. |

Não proponho adicionar bandeja do sistema nesta entrega: manter a comunicação USB em segundo plano exige decidir como apresentar a sessão ativa e tratar o fechamento do editor.

## Referências oficiais

- [Temas e recursos dinâmicos](https://wpfui.lepo.co/documentation/themes)
- [SystemThemeWatcher](https://wpfui.lepo.co/api/Wpf.Ui.Appearance.SystemThemeWatcher.html)
- [InfoBar](https://wpfui.lepo.co/api/Wpf.Ui.Controls.InfoBar.html)
- [SnackbarService](https://wpfui.lepo.co/api/Wpf.Ui.SnackbarService.html)
- [Serviços WPF UI](https://wpfui.lepo.co/api/Wpf.Ui.html)

O projeto continua fixado no WPF UI 4.3.0. A implementação foi conferida também no código-fonte dessa versão. As sugestões futuras não fazem parte da entrega atual.
