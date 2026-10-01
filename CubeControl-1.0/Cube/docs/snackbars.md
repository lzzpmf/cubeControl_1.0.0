# Notificações do CubeControl

Implementação baseada no `SnackbarService` e no `SnackbarPresenter` do WPF UI 4.3.0, preservando o template, as cores semânticas e o botão de fechar do componente.

| Evento | Aparência | Duração |
| --- | --- | --- |
| Pedal identificado e leitura concluída | Success | 5 segundos |
| Perda de comunicação ou desconexão manual | Danger | 8 segundos |
| Falha de uma operação solicitada pelo usuário | Danger | 8 segundos |
| Presets gravados e confirmados, exportação, backup ou restauração concluídos | Success | 5 segundos |
| Banco importado e aplicado, ainda sem gravação permanente | Caution | 5 segundos |
| Releitura de presets concluída | Success | 5 segundos |

As confirmações existentes de associações e imagens locais continuam em Success. Falhas ao salvar preferências ou importar imagens usam Danger.

Não emitir mensagens para navegação, seleção de preset, knobs, monitoramento USB ou prévias automáticas de IR. Uma conexão confirmada gera um aviso; consultas periódicas e novas tentativas automáticas não repetem notificações. A mensagem mais recente substitui a anterior, sem fila de estados antigos.

Snackbar informa o resultado e não exige resposta nem move o foco. Problemas persistentes continuam no estado/aviso do editor. Confirmações antes de substituir conteúdo ou descartar alterações continuam em diálogos. O aviso de importação não afirma que o banco foi gravado no pedal.

Textos novos disponíveis em pt-BR e en-US. Ícones acompanham a gravidade; a região informa tecnologias assistivas. Validação direcionada: `--snackbar-only` e `--auto-connect-only` em `Cube.Desktop.Tests`.

Referências oficiais: [SnackbarService](https://github.com/lepoco/wpfui/blob/main/src/Wpf.Ui/SnackbarService.cs), [Snackbar](https://wpfui.lepo.co/api/Wpf.Ui.Controls.Snackbar.html).
