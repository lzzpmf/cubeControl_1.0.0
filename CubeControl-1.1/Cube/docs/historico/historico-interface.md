# Histórico de interface do cubeControl

[Documentação](../README.md) · [Design system atual](../DESIGN_SYSTEM.md)

> Arquivo histórico da documentação anterior à revisão de 01/10/2026. O conteúdo abaixo conserva diretrizes e decisões de versões anteriores, inclusive regras substituídas e testes que não foram repetidos nesta revisão. Termos como atual e entregue referem-se à etapa descrita. Para orientação vigente, use o design system atual.

**Documento original: CubeControl — Design System**

Versão do documento: 1.3 · 18/09/2026

## 1. Finalidade e uso

Referência para evoluir a interface Windows do CubeControl com consistência, preservando sua identidade e o comportamento do Cube Baby. Destina-se a design, desenvolvimento e revisão de UX/UI.

Este documento define diretrizes de evolução; não declara que todos os critérios já estão validados. A versão 3.3.0 implementa a primeira rodada de melhorias nas sete telas. Decisões de implementação e exceções estão registradas ao final. O comportamento real e as boas práticas dos controles WPF UI 4.3.0 têm precedência sobre uma aplicação literal deste guia; registrar a justificativa quando houver divergência.

**Base inspecionada:** aplicativo 3.3.31, .NET 10/WPF, WPF UI 4.3.0, temas Sistema/Claro/Escuro, localização pt-BR/en-US, controles próprios de knob, imagem e forma de onda. As sete capturas fornecidas complementam a leitura do código; não substituem testes de interação.

### Princípios

1. **O timbre é a tarefa principal.** Edição sonora precede personalização visual e diagnóstico.
2. **Estado explícito.** Selecionar um módulo, ativar um efeito, editar um preset e gravar no aparelho são ações distintas.
3. **Preservação da identidade.** Manter navegação superior, cadeia de seis cards visuais e cinco blocos do aparelho, imagens de equipamentos e controles musicais. Evitar redesign amplo sem necessidade.
4. **Verdade sobre o hardware.** Não inferir preset audível, confirmação de gravação, unidade física ou identificação de equipamento sem evidência disponível.
5. **Precisão sem excesso técnico.** Expor faixa, destino e consequência; recolher hashes, caminhos e detalhes de protocolo.
6. **Acessibilidade desde o componente.** Mouse, teclado e tecnologia assistiva devem permitir concluir as mesmas tarefas.

## 2. Fundamentos visuais

### Cores e temas

Usar recursos semânticos via `DynamicResource`. Não inserir cores literais em novas telas quando já existir um recurso adequado. Os valores abaixo são a base existente em `ThemeService`, e não uma nova paleta.

| Recurso | Claro | Escuro | Uso |
| --- | --- | --- | --- |
| `CubeCanvas` | `#F3F3F3` | `#202020` | Fundo da janela |
| `CubeCard` | `#FBFBFB` | `#2B2B2B` | Superfície principal |
| `CubeInset` | `#F5F5F5` | `#252525` | Área de identidade do equipamento |
| `CubeWell` | `#EFEFEF` | `#1C1C1C` | Área técnica e forma de onda |
| `CubeText` | `TextFillColorPrimaryBrush` | `TextFillColorPrimaryBrush` | Texto principal; recurso nativo WPF UI |
| `Muted` | `TextFillColorSecondaryBrush` | `TextFillColorSecondaryBrush` | Texto secundário ativo; nunca usar tertiary/disabled para descrições |
| `CubeBorder` | `#E0E0E0` | `#404040` | Contorno e separador |
| `CubeSelected` | `#E9E9E9` | `#3B3B3B` | Fundo de seleção persistente |
| `CubeSelectedText` | `#1B1B1B` | `#FFFFFF` | Texto sobre seleção |
| `CubeAccent` | `#626262` | `#BDBDBD` | Destaque próprio do editor e seleção |
| `CubeFocus` | `#1A1A1A` | `#FFFFFF` | Indicador de foco |
| `CubeWarning` | `SystemFillColorCautionBrush` | `SystemFillColorCautionBrush` | Atenção com semântica nativa WPF UI |
| `CubePurple` | `#67508F` | `#B4A4D6` | Recurso existente; não usar como ação primária alternativa |
| `CubeDisabledText` | `TextFillColorDisabledBrush` | `TextFillColorDisabledBrush` | Apenas indisponibilidade; GrayText do Windows em alto contraste |

O destaque dos controles WPF UI acompanha o Windows. O vermelho das capturas não é uma cor fixa da marca e não deve ser codificado como tal. Preservar essa integração; diferenciar funções pela combinação de aparência, posição, texto e estado, mesmo quando o destaque do sistema for vermelho ou verde.

Para sucesso, atenção e erro, preferir as aparências semânticas dos componentes WPF UI, acompanhadas de ícone e mensagem. Novos recursos de cor exigem pares claro/escuro e comportamento em alto contraste.

| Módulo | Claro | Escuro |
| --- | --- | --- |
| Amp | `#82551F` | `#D5AD7D` |
| Modulação | `#226E59` | `#80B8A8` |
| Delay | `#2D638B` | `#82ACD2` |
| Reverb | `#765394` | `#B49ACF` |
| Cabinet | `#726021` | `#C5B77F` |

Cores de módulo identificam categoria, não ativação ou gravidade. Preservar os recursos `CubeKnobBase`, `CubeKnobTop`, `CubeKnobBottom` e `CubeKnobTrack` no desenho dos knobs. Fotos não devem receber filtros ao trocar o tema. Em alto contraste, priorizar as cores de sistema já adotadas pelo serviço de tema.

### Tipografia e medidas

Todas as medidas de layout são em DIPs do WPF, não pixels físicos. Usar Segoe UI; Consolas apenas para logs e conteúdo técnico que necessite alinhamento monoespaçado.

A partir da versão 3.3.22, por solicitação do usuário, o conteúdo recebe uma escala única de layout de 90%. As medidas deste documento são anteriores à transformação. A janela abre em 1280 × 720 DIPs, limitada à área útil do monitor; a barra de título e os diálogos nativos do Windows não recebem a escala. Essa decisão substitui a proibição anterior de redução global por escala, mantendo a composição responsiva e permitindo rolagem.

| Papel | Tamanho alvo | Peso |
| --- | --- | --- |
| Título da página | 24 | SemiBold |
| Nome do equipamento | 22 | SemiBold |
| Título de seção | 20 | SemiBold |
| Nome do módulo na cadeia | 16 | SemiBold |
| Botões, rótulos e texto principal | 14 | Normal |
| Ajuda e metadados secundários | 12 | Normal |

Esta escala é uma diretriz de evolução: existem textos de 10–13 DIPs no código atual. Não aplicar substituição global de tamanhos. Priorizar instruções, estados e mensagens essenciais. Evitar caixa alta em frases e textos longos.

- Espaçamento base: 4, 8, 12, 16, 24 e 32 DIPs.
- Margem externa: 24 DIPs em janela ampla; 16 em modo compacto. Workspace centralizado e limitado a 1440 DIPs para evitar dispersão em monitores largos.
- Padding de card: 24 DIPs; 16 em grupos compactos. Distância entre grupos: 16–24.
- Preservar raios atuais de 14 DIPs em painéis e 10 em seletores. Controles comuns seguem o tema WPF UI.
- Contorno normal: 1 DIP. Foco: indicador próprio de 2 DIPs, sem alterar o tamanho ocupado.
- Alvo interativo: mínimo de 32 × 32 DIPs; altura preferencial de 36–40 para comandos. Uso por toque requer alvo de pelo menos 44 × 44.
- Não esticar botões para ocupar espaço sem finalidade. Evitar sombras fortes, animação de escala ou hover que mova elementos.

## 3. Layout e navegação

Preservar três áreas: **Pedaleira**, **IRs**, **Dispositivo e ajuda**. Selecionar uma área ou módulo não deve enviar comandos ao pedal nem recriar a sessão USB.

### Estrutura da janela

1. Barra de título nativa/Fluent com comandos da janela.
2. Cabeçalho compacto com identidade, dispositivo e preferências de tema/idioma.
3. Faixa de presets: preset em edição, estado de persistência, saída e ação de gravação relacionada.
4. Navegação e conteúdo principal com altura flexível.
5. Rodapé reservado no layout para comandos auxiliares e estado global pertinente.

Aproximar a gravação da faixa de presets e manter tema/idioma com menor destaque. O conteúdo deve ocupar uma linha `*` de Grid; cabeçalho e rodapé usam `Auto`. Não sobrepor rodapé, avisos ou barra de progresso ao conteúdo rolável.

### Adaptação à largura

- A janela tem tamanho inicial 1280 × 720 e mínimo 640 × 480 DIPs, limitados à área útil do monitor quando ela for menor. Em áreas pequenas, o workspace admite rolagem integral para manter comandos acessíveis.
- O editor usa divisão 40%/60% e mantém as colunas até 740 DIPs disponíveis. Abaixo disso, apresenta os controles sonoros antes da identidade visual. Os painéis de IRs usam 50%/50% e empilham abaixo de 900 DIPs; os limiares dependem do conteúdo, não de uma regra única.
- Permitir quebra de linhas em grupos de botões; títulos e mensagens devem crescer verticalmente.
- Manter a cadeia em ordem 01–06. Se seis cards não couberem de forma legível, usar duas linhas em ordem de leitura, sem reduzir fontes para forçar o encaixe.
- Rolagem vertical deve revelar o último controle e sua mensagem inteira. Evitar rolagem horizontal. Em altura lógica da área cliente inferior a 764 DIPs ou largura útil do workspace inferior a 1000, usar rolagem externa e desativar a rolagem interna da página. Em janelas amplas, o rodapé permanece reservado e só o conteúdo rola.
- Em janelas baixas, a altura das imagens pode se adaptar. A redução uniforme autorizada é de 10%, aplicada uma única vez; não introduzir outras reduções globais ou reorganizações para eliminar a rolagem.
- Se a área útil do Windows for menor que o mínimo atual, adequar o mínimo e a composição compacta durante a implementação; não aceitar ações inacessíveis fora da tela.

## 4. Componentes e estados

### Matriz de estados

| Estado | Representação e comportamento |
| --- | --- |
| Normal | Superfície e contorno neutros |
| Hover | Alteração sutil de superfície, sem reproduzir seleção ou mudar geometria |
| Pressionado | Feedback breve conforme o controle WPF UI |
| Foco de teclado | Contorno próprio, visível inclusive sobre item selecionado |
| Selecionado | Fundo `CubeSelected`, texto correspondente e semântica de seleção acessível |
| Efeito ligado/desligado | Switch e texto explícito; independente do módulo selecionado |
| Indisponível | Aparência desabilitada e motivo próximo quando não for evidente |
| Processando | Indicador e descrição da operação; impedir comandos conflitantes |
| Erro | Mensagem persistente com consequência e próximo passo disponível |

### Botões

- Botões de comando usam `CubeButtonStyle`, derivado do template nativo WPF UI: altura mínima de 40 DIPs, fonte de 14 DIPs, padding de 16 DIPs nas laterais e 8 DIPs acima/abaixo, conteúdo centralizado. A escala global existente continua sendo aplicada uma única vez.
- Diálogos mantêm a mesma altura e padding vertical. Em modo compacto, o padding horizontal pode cair para 12 DIPs e o texto pode quebrar; todos os botões da linha acompanham a maior altura necessária.
- Botões somente com ícone usam 40 × 40 DIPs. Cards, imagens clicáveis, seletores e controles nativos de janela mantêm suas dimensões próprias.

- **Primário:** ação principal do contexto, usando `ui:Button` e aparência Primary. Na Pedaleira, gravar presets. Em IRs, a ação principal depende da etapa; a gravação global de presets permanece secundária nesse contexto.
- **Secundário:** selecionar arquivo, criar backup, abrir galeria e atualizar inventário.
- **Discreto:** preferências, personalização de imagem e detalhes técnicos.
- **Destrutivo:** restauração ou substituição relevante exige texto específico, destino e confirmação prevista pelo fluxo. Não confundir destaque vermelho do sistema com semântica destrutiva.
- Usar verbo + objeto. Reticências indicam que uma seleção ou configuração adicional será solicitada.
- Botão somente com ícone exige tooltip e nome acessível. Usar `SymbolIcon` quando apropriado em vez de caracteres de fonte ambíguos.
- Motivos de indisponibilidade importantes devem aparecer na tela; tooltip sozinho não basta.

### Presets e cards de efeitos

- A/B/C são seleção exclusiva do preset **em edição**, não indicação confirmada do footswitch ativo.
- Cards de módulo selecionam o editor; clicar neles não ativa o efeito.
- Cada card informa posição, nome e resumo do estado. Identificar efeitos desligados por texto.
- Estado de hover nunca deve parecer uma segunda seleção. Foco e seleção podem coexistir, com tratamentos diferentes.
- Nome longo pode ser abreviado no card com tooltip; o painel de detalhes deve permitir ler o nome completo.

### Knobs

- Exibir knob e rótulo, sem campo numérico separado, setas de incremento ou texto de faixa. O valor fica disponível no tooltip e na automação acessível.
- Preservar arraste, roda sob o cursor, setas, Home/End e agrupamento de desfazer. Fora do knob, a roda continua rolando a página.
- O volume principal do preset usa knob de 104 × 104 DIPs e 16 DIPs de separação do título. Mensagens de estado ficam em linha independente abaixo dos controles, sem alterar a posição do alvo durante o gesto.
- Usar limites reais do ViewModel. Não inventar unidades físicas sem conversão comprovada.
- Reaproveitar AutomationPeer/RangeValue para nome, valor, limites e alterações.
- Permitir preparar ajustes dos efeitos desligados, sem ativá-los ou alterar o efeito ativo.

### Switches e cadeia de efeitos

- Cada card contém o interruptor do efeito. Selecionar o card apenas abre seu editor.
- Ordem visual: Amp, Chorus, Phaser, Delay, Reverb, Cabinet.
- Chorus e Phaser são cards distintos, mas compartilham o parâmetro MIDI de modulação. Ligar um desliga o outro; cada um preserva seu ajuste preparado. Navegar ou preparar o tipo inativo não envia mudanças ao efeito ativo.
- No Amp, o switch deve indicar Tone: sua função não é bypass do amplificador.
- Associação automática é uma preferência e permanece no painel de associações.

### Imagens e galerias

- Imagem serve ao reconhecimento; preservar proporção, sem distorção ou corte essencial.
- Separar escolher equipamento de personalizar sua imagem. Rótulos preferidos: “Escolher amplificador”, “Escolher gabinete” e “Personalizar imagem…”.
- Restauração de imagem fica junto da personalização, com nome “Restaurar imagem padrão”.
- Preservar prioridade existente: foto personalizada válida → foto padrão → ilustração genérica.
- IR personalizado não herda nome ou foto do equipamento que antes ocupava seu slot. Metadados acompanham o conteúdo identificado.
- Galeria mantém escolha preliminar e botão Aplicar; Cancelar não envia a escolha ao dispositivo.

### Feedback e diálogos

| Necessidade | Componente/diretriz |
| --- | --- |
| Confirmação breve de exportação, backup ou gravação concluída | Snackbar; não emitir a cada ajuste de knob |
| Desconexão, aplicação parcial ou verificação inconclusiva | InfoBar persistente, com ação existente de recuperação quando aplicável |
| Operação sem total conhecido | ProgressRing com nome da operação |
| Transferência com total conhecido | ProgressBar vinculada ao progresso real |
| Sobrescrita/restauração | ContentDialog com destino, consequência, backup e botões específicos |

Snackbar e InfoBar foram adotados na versão 3.3.0. Confirmações breves usam Snackbar; erros e orientação contextual usam InfoBar. Estado que afete decisões deve continuar consultável após a notificação desaparecer. Não mostrar 100% como sucesso antes de terminar a verificação exigida pelo fluxo. Erros não desaparecem por temporizador: falhas de sessão permanecem até recuperação; mensagens de operação não bloqueante podem ser substituídas quando o usuário inicia uma nova tentativa.

## 5. Linguagem e estado do aparelho

Usar português direto na base e atualizar o catálogo inglês junto com alterações de texto. Preservar nomes comerciais, nomes de arquivos e termos musicais reconhecíveis como Delay, Reverb, Chorus e Phaser. Usar “gabinete / IR” para introduzir Cabinet e “associação de gabinete” para explicar Match Cabinet.

| Conceito | Regra e exemplo |
| --- | --- |
| Contexto de edição | “Editando: preset A” |
| Escuta | “Para ouvir, selecione A no pedal”; não afirmar preset audível sem confirmação |
| Alteração local | “Alterações locais”, quando ainda não houve envio |
| Envio concluído | “Enviado ao aparelho · não gravado”, apenas quando o transporte confirmar o envio |
| Gravação | “Gravar os 3 presets no aparelho”, preservando o alcance real da ação A/B/C |
| Resultado incerto | “Não foi possível confirmar a gravação”; não substituir por “Salvo” |
| Prévia de IR | “Prévia ativa no pedal” somente conforme estado conhecido pelo fluxo |
| Releitura | “Reler presets do aparelho”; explicar quando substitui a edição local |
| Desfazer | “Desfazer último ajuste”; não prometer desfazer gravação de IR ou restauração |

Esses exemplos não autorizam alterar protocolo, confirmação ou persistência. Vincular cada mensagem ao estado efetivamente disponível. Não equiparar envio MIDI a confirmação de reprodução ou persistência.

Mensagens de erro devem responder: o que aconteceu, o que foi preservado ou ficou incerto e qual ação o usuário pode executar. Detalhes técnicos ficam expansíveis. Evitar repetir um sucesso antigo de IR em todas as páginas.

Usar `loc:Translate` e os catálogos existentes. Mudanças de rótulo exigem revisar chaves/traduções, pois o português é usado como chave. Idioma, tema e imagens não alteram parâmetros ou enviam MIDI.

## 6. Aplicação às sete telas

| Tela | Direção de design |
| --- | --- |
| Amp | Manter imagem e Gain/Tone; separar escolha do amp, imagem e associação de gabinete. Explicar o alcance do switch Tone. |
| Modulação | Tipo exclusivo, ativação separada e faixa visível. Descrever “Ajuste” segundo sua função real, sem inventar parâmetro físico. |
| Delay | Título funcional “Delay”; Tempo, Repetições e Mistura como parâmetros, com faixa/unidade validada. |
| Reverb | Preservar posição previsível do controle; reduzir espaço decorativo quando necessário, sem adicionar controles inexistentes. |
| Cabinet | Destacar slot, nome e estado de identificação. Separar inventário de slots de identificação de equipamentos; oferecer acesso direto a IRs. |
| IRs | Fluxo selecionar WAV → preparar/ouvir → escolher destino → gravar/verificar. Prévia não grava permanentemente. |
| Dispositivo e ajuda | Exibir conexão e organizar modos em “Modo / Comando físico / Resultado”. Recuperação e diagnóstico em grupos próprios. |

### Regras específicas de IRs

- Mostrar arquivo, slot de destino e conteúdo atual antes da gravação.
- Manter backups e confirmações existentes; não simplificar o visual removendo proteção de dados.
- Explicar indisponibilidade de gravar junto ao botão, usando o motivo real do comando.
- Diferenciar arquivo original de conteúdo preparado. Mostrar que serão usadas 512 amostras quando esse for o processamento efetivo; contagens de descarte e formatos detalhados podem ficar em detalhes.
- Informar quando a forma de onda representa dados preparados e quando sua amplitude foi ajustada apenas para visualização.
- “Distância do microfone” mantém a escala do recurso existente, sem inventar distância em centímetros. Tornar perceptível a indicação de ausência de som em 100%.
- Informar que alterações de distância valem para a próxima prévia ou gravação, conforme o comportamento atual.
- Após gravar, mostrar verificação e eventual necessidade de selecionar outro gabinete e voltar ao slot; não ocultar instrução ainda necessária dentro de uma notificação temporária.

## 7. Acessibilidade e critérios de aceite

Metas de contraste: pelo menos 4,5:1 em texto comum, 3:1 em texto grande e 3:1 para indicadores essenciais de controles/foco contra fundos adjacentes. Medir combinações renderizadas, inclusive destaque variável do Windows; a tabela de cores não comprova conformidade.

- [ ] Todas as tarefas disponíveis por teclado; Tab/Shift+Tab em ordem de leitura e sem aprisionamento.
- [ ] Foco visível em presets, cards, switches, campos, knobs e diálogos; foco retorna ao acionador ao fechar diálogo.
- [ ] Nomes acessíveis localizados; leitores de tela distinguem tipo selecionado, ativação, valor e limites.
- [ ] Estados importantes não dependem apenas de cor, ícone, tooltip ou animação.
- [ ] Claro, escuro e alto contraste legíveis; Sistema acompanha o Windows sem alterar a escolha global do usuário.
- [ ] Revisão em 100%, 125%, 150% e 200% de escala, com registro da área útil e das dimensões lógicas testadas.
- [ ] Revisão em janela ampla e compacta, inclusive monitor 1366 × 768: nenhuma ação inacessível ou mensagem encoberta.
- [ ] pt-BR/en-US e nomes longos de IR não cortam comandos essenciais.
- [ ] Estados conectado, desconectado, ocupado, erro, sem alterações e com alterações são distinguíveis.
- [ ] Efeito desligado com ajustes preparados não aparenta estar sendo ouvido.
- [ ] Campos inválidos não transmitem valores; ajuste por teclado/roda mantém desfazer coerente.
- [ ] Prévia, backup, gravação e verificação de IR não se confundem; destino e resultado permanecem claros.
- [ ] Navegação, tema, idioma e personalização visual não disparam comandos de áudio.

Na implementação, executar testes direcionados ao comportamento alterado e inspeção visual em execução. Testes de lógica ou build não substituem aceitação visual. Validação offline não comprova resposta física do pedal; registrar separadamente o que foi testado com hardware.

## 8. Manutenção e referências

Reutilizar estilos e componentes existentes antes de criar novos. Centralizar futuros tokens de dimensão/tipografia; não realizar migração global só para uniformizar números. Não atualizar WPF UI ou alterar protocolo como consequência implícita de uma melhoria visual.

Regras novas ou exceções devem ser registradas aqui com motivo, alcance e revisão dos critérios afetados. A documentação descreve o padrão desejado; o código e os testes comprovam o que foi entregue. Divergências devem ser tratadas em tarefas explícitas.

### Fontes locais inspecionadas

- [Estilos compartilhados](../../windows/Cube.Desktop/App.xaml) e [recursos de tema](../../windows/Cube.Desktop/Appearance/ThemeService.cs).
- [Composição das telas](../../windows/Cube.Desktop/MainWindow.xaml), [painel adaptativo](../../windows/Cube.Desktop/Controls/AdaptivePanel.cs) e [knob](../../windows/Cube.Desktop/Controls/Knob.cs).
- [Versões e dependências](../../windows/Cube.Desktop/Cube.Desktop.csproj).
- [Comportamentos do produto](../../windows/README.md) e [guia de localização](../../windows/Cube.Desktop/Localization/README.md).
- [Integrações WPF UI já avaliadas](../desenvolvimento/wpf-ui-integracoes.md).

### Referências oficiais

- [WPF UI](https://wpfui.lepo.co/) — conferir APIs na versão 4.3.0 fixada no projeto antes da adoção.
- [NumberBox](https://wpfui.lepo.co/api/Wpf.Ui.Controls.NumberBox.html), [InfoBar](https://wpfui.lepo.co/api/Wpf.Ui.Controls.InfoBar.html) e [Snackbar](https://wpfui.lepo.co/api/Wpf.Ui.Controls.Snackbar.html).
- [Foco no WPF](https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/focus-overview) e [acessibilidade Windows](https://learn.microsoft.com/en-us/windows/apps/design/accessibility/accessibility-overview).

### Decisões aplicadas na versão 3.3.0

| Decisão | Motivo e limite |
| --- | --- |
| `ParameterNumberBox` deriva de `NumberBox` e mantém seu template nativo | A versão 4.3.0 limita valores silenciosamente e valida o rascunho anterior ao colar. O adaptador recusa entradas inválidas e adia a confirmação da colagem até Enter/perda de foco, preservando incrementos e desfazer. |
| Chorus/Phaser usam RadioButton com tema WPF UI | A seleção exclusiva fica reconhecível e acessível sem criar outro botão primário ou switch de ativação. |
| Controles antes da imagem na composição estreita | Prioriza ajuste sonoro; a ordem de Tab acompanha a composição. Mantém a divisão original em janelas amplas. |
| Limiar próprio para faixa de presets (760), editor (740) e IRs (900) | A revisão visual mostrou que aplicar 900 a todos os grupos criava espaço excessivo no cabeçalho e escondia os parâmetros. |
| Tipografia e espaçamentos migram de forma localizada | Preserva estilos nativos da biblioteca e evita substituir valores indiscriminadamente; títulos, faixas e mensagens essenciais recebem tratamento consistente. |
| Rodapé contém comandos e progresso; último resultado fica em Dispositivo e ajuda | Evita repetir um sucesso antigo de IR em todas as páginas. Confirmações pontuais aparecem em Snackbar e o resultado de IR permanece na própria página. |
| Prévia pendente é distinta de prévia confirmada | Snapshot de recuperação ou início de transferência não comprova reprodução no aparelho. |

Capturas offline e com transporte MIDI simulado não comprovam áudio real, DPI nativo do monitor ou leitura por Narrator. Registrar esses limites no relatório da entrega; não marcar automaticamente todo o checklist como concluído.

### Revisão 3.3.1

A pedido do usuário, campos numéricos e faixas foram removidos de todos os knobs, inclusive saída. Esta decisão substitui a regra anterior de entrada numérica permanente. Os interruptores foram movidos para os cards. Chorus e Phaser passaram a ter cards independentes, preservando a exclusividade e o protocolo do aparelho. As decisões 3.3.0 acima são histórico, não exigência para restaurar NumberBox ou RadioButtons na interface.

### Ícone por tema — 3.3.2

Usar os arquivos originais de `Assets/Images/Icons`: `cubeBabyBlack.png` no tema claro e `cubeBabyWhite.png` no escuro. Cabeçalho e barra de título interna compartilham o recurso dinâmico `CubeAppIcon`. O ícone nativo da janela/barra de tarefas usa `CubeWindowIcon`, independente do tema do app. No modo Sistema, considerar o tema efetivo resolvido pelo WPF UI, inclusive mudanças durante a sessão. Em alto contraste, selecionar pela luminosidade do fundo do sistema. Os PNGs são recursos embutidos, sem depender de caminhos locais.

### Espera centralizada — 3.3.3

O indicador de operação fica centralizado na área útil da janela, acima do conteúdo e abaixo dos diálogos. Mensagens breves ligadas ao universo musical alternam a cada quatro segundos, sem simular etapas técnicas ou progresso. Transferências de IR mantêm o estado real visível. Ao concluir ou falhar, o indicador desaparece; o resultado segue os mecanismos existentes de feedback.

### Ícone da barra de tarefas — 3.3.5

O ícone nativo da janela segue o tema do shell do Windows (`SystemUsesLightTheme`): preto no Windows claro e branco no Windows escuro. O tema manual do app não altera essa escolha. Mudanças do Windows atualizam esse ícone durante a sessão, inclusive fora do modo Sistema. Cabeçalho e barra de título desenhada dentro do app continuam seguindo o tema do app.

### Características de cabinets — 3.3.6

A linha abaixo do nome dos cabinets identificados apresenta pelo menos duas características do modelo de referência, em vez do estado Original identificado. Usar dados de fabricantes documentados em [cabinet-characteristics.md](../referencias/cabinet-characteristics.md), com quebra de linha para evitar cortes. Vincular pelo conteúdo identificado, nunca pelo número do slot ou nome do arquivo. IRs desconhecidos mantêm sua indicação factual; não inventar especificações.

### Precisão do carregamento — 3.3.7

A Pedaleira deve estar selecionada desde a abertura, sem exigir clique para revelar conteúdo. O aviso central combina mensagem temática com etapa real: descoberta USB, leitura A/B/C e leitura dos oito cabinets. O progresso de inventário conta slots efetivamente lidos; não usa temporizador ou estimativa. Presets visíveis não significam conclusão do inventário. Edição continua bloqueada até o fim da operação serializada. Respostas de progresso atrasadas não podem sobrescrever a conclusão. Os intervalos USB existentes permanecem preservados.

### Tema escuro neutro — 3.3.10

A base escura usa cinzas neutros inspirados na referência visual do Windows: canvas #202020, cards #2B2B2B, áreas internas #252525, bordas #404040 e seleção #3B3B3B. Textos, foco, seleção e superfícies dos knobs também são neutros. As cores de identificação dos efeitos e de feedback permanecem pontuais. Esta regra substitui os tons esverdeados anteriores do tema escuro; não altera o tema claro nem as cores de alto contraste do sistema.

### Imagens de equipamentos — 3.3.11

Área de imagem de 210 DIPs no editor e 135 DIPs na galeria, preservando proporções. Hover amplia 6% com RenderTransform em 180 ms, sem alterar medidas do layout. Manter respiro para não cobrir títulos e controles. Respeitar a preferência do Windows para animações e usar interpolação de alta qualidade.

### Seletores de equipamento — 3.3.12

Imagens do amplificador e gabinete abrem o respectivo seletor quando a edição está disponível; Enter e Espaço oferecem acesso pelo teclado. Clique simples seleciona, clique duplo no item aplica e fecha; Aplicar seleção permanece disponível. X, Cancelar, Escape e clique fora fecham sem aplicar. Preservar o filtro dos amplificadores. Fechamento externo é exclusivo dos seletores, não de confirmações destrutivas.

### Carrossel de equipamentos — 3.3.14

Os seletores de amplificador e gabinete usam uma faixa horizontal com setas anterior/próximo e contador de posição. As setas percorrem os itens do filtro ativo, retornando ao início após o último. Navegação apenas destaca; aplicar exige clique duplo ou Aplicar seleção. No carrossel, imagens de amps usam 180 DIPs de altura e gabinetes mantêm 240 DIPs. Filtro de amps, fechamento externo e X permanecem disponíveis.

### Rolagem horizontal — 3.3.15

A cadeia principal usa uma única faixa horizontal, substituindo a quebra em duas linhas. Cadeia e carrosséis aceitam roda do mouse com deslocamento em pixels e desaceleração suave, acumulando deltas fracionários. Rolar não seleciona nem aplica efeitos. Respeitar limites da faixa e a preferência de animação do Windows; interromper a animação ao clicar, usar teclado ou descarregar a tela. A roda sobre knobs mantém o ajuste do parâmetro.

### Cabeçalho e seleção direta — 3.3.16

O cabeçalho separa identidade, preferências e conexão. Tema e Idioma têm 40 DIPs; conectar e carregar tem destaque nativo. A faixa de edição usa seletores A/B/C de 48 DIPs, saída de 64 DIPs e gravação de 44 DIPs. Estado do preset e pendências do banco são independentes; o destaque de gravação considera todo A/B/C, sem depender da aba. Abaixo de 1.000 DIPs úteis a conexão ocupa outra linha; em espaços ainda menores, preferências e gravação também são empilhadas, mantendo rolagem e acesso.

Esta regra substitui o carrossel dos seletores introduzido em 3.3.14: amplificadores e gabinetes voltam à grade com rolagem vertical, mantendo filtro de amps, clique duplo, aplicação explícita e fechamento externo. A roda sobre a imagem principal troca diretamente o equipamento, acumulando deltas e respeitando os limites e bloqueios existentes. A rolagem horizontal suave da cadeia permanece.

Fechar seletores usa SymbolRegular.Dismiss24, tamanho visual 16 DIPs, em botão transparente de 32 DIPs, sem borda permanente, com tooltip e nome acessível Fechar. Estados de foco e hover seguem o WPF UI. Não usar o caractere tipográfico × como ícone.

### Navegação da cadeia pela roda — 3.3.17

Na faixa de cards, a roda seleciona o efeito: para baixo vai à esquerda; para cima vai à direita. A seleção atualiza o painel de controles e mantém o card visível. Acumular deltas fracionários, parar nas extremidades e limpar o acumulador ao sair da faixa. Esta navegação substitui a rolagem horizontal livre pela roda nessa região; não alterna interruptores nem envia comandos MIDI. A roda nas imagens de amplificador/gabinete mantém a troca de equipamento existente.

### Navegação principal — 3.3.18

Pedaleira, IRs e Dispositivo e ajuda ficam em uma faixa própria com superfície e borda dos recursos do tema, raio de 10 DIPs e distância de 16 DIPs do conteúdo. Preservar NavigationView em modo Top e seus templates nativos de seleção, hover e foco. Os itens têm altura mínima de 48 DIPs, texto de 16 DIPs, peso semibold apenas quando ativos e ícones Fluent de 20 DIPs acompanhados dos rótulos. Manter traduções e nomes acessíveis. Não fixar cores de fundo ou destaque: respeitar os temas e alto contraste.


### Adequação WPF UI — revisão de 18/09/2026 (base 3.3.31)

Esta seção descreve a implementação atual e prevalece sobre as decisões históricas incompatíveis acima. A auditoria completa, os testes e seus limites constam em `wpf-ui-audit.md` (artefato histórico não incluído nesta árvore).

- A navegação principal permanece um `ListBox` de cards, com quebra em largura reduzida; substitui o `NavigationView` descrito em 3.3.18. Presets e cadeia continuam seletores exclusivos, com foco independente da seleção. Conteúdo secundário e ícones respeitam o par de cores do item selecionado, inclusive em alto contraste.
- `QuickAction` utiliza o template de `CardAction` do WPF UI 4.3.0, sem chevron, mantendo superfície transparente em repouso e a composição existente. Estilos de texto secundário e ícones de seleção são compartilhados.
- A imagem de amplificador/gabinete é conteúdo de um botão WPF UI acessível. Ilustrações sem galeria não entram na ordem de Tab nem se anunciam como ações. O zoom de 6% da revisão 3.3.11 foi removido; fotos mantêm proporção, cores e tamanho, com feedback nativo no botão.
- O microfone mantém sua cena e aplica ao concluir arraste, tecla ou alteração por tecnologia assistiva. Sua automação expõe intervalo normalizado de 0–100; esse valor não representa centímetros. Foco e indisponibilidade têm contorno visível. Knobs mantêm gestos e sensibilidade; alterações por automação integram o desfazer.
- A escala continua única de 90%, incluindo menus e diálogos conforme `UiDensity`. O botão de fechar galeria e o link de conexão passam a 36 DIPs, ou 32,4 DIPs efetivos. Metadados de galeria e associações usam 12 DIPs antes da escala.
- Avisos usam `Wpf.Ui.Controls.MessageBox` com proprietário, localização e proteção contra duplicação. Confirmações e seletores de arquivo preservam seus fluxos. O fechamento externo da galeria fica isolado em `GalleryDialogDismissal`, com contrato de template testado na versão instalada; não se aplica a confirmações destrutivas.
- O detalhe real de operação é uma região acessível com anúncio de mudança. Frases decorativas rotativas não geram anúncios de região ao vivo. Progresso de IR, inventário e forma de onda têm nomes acessíveis.
- São exceções intencionais os desenhos musicais, a paleta Cube, os cards de seleção, os painéis adaptativos e a escala de 90%. Nenhuma dessas exceções autoriza alterar USB/MIDI, persistência, nomes de equipamentos ou a estrutura das páginas.

### Menu de opções do cabeçalho — 18/09/2026

Tema, Idioma e Detalhes da conexão ficam no menu Mais opções, aberto pelo botão de reticências de 40 DIPs (36 DIPs efetivos). Tema e Idioma são submenus com a seleção atual marcada. Estado da conexão e Conectar permanecem visíveis. Esta regra substitui os botões separados descritos acima.

Usar Button WPF UI transparente e ContextMenu com templates da biblioteca, ícones Fluent, textos localizados e escala única de 90%. Preservar acionamento por teclado, Escape e retorno do foco ao botão ao fechar o menu ou os detalhes USB. O botão permanece na primeira linha em janelas estreitas; a conexão mantém seu reflow. O acesso aos detalhes na página de ajuda continua disponível.

### Indicadores de operação — 18/09/2026

O painel central usa um único indicador principal: ProgressRing em esperas sem medida de avanço, ProgressBar determinada para inventário (0–8 cabinets) ou transferência de IR (bytes da etapa atual). A barra de IR fica no painel central, disponível em qualquer página, sem duplicação na página de IRs. Mudanças de etapa podem reiniciar a barra; ela não representa o percentual total da restauração. Backups e restaurações em lote identificam o slot atual e o total de oito slots.

A busca de portas usa barra indeterminada junto ao estado da conexão, sem painel central. Operações concluídas ou com falha retiram o indicador. Progresso sem total conhecido usa ring, sem percentual artificial. Textos e nomes acessíveis seguem a localização; protocolo, backups e confirmações permanecem preservados.

### Superfícies sem contorno — 18/09/2026

Cards, presets, tiles de equipamentos e painel de processamento usam fundos e espaçamento como separação nos temas claro e escuro. O recurso CubeSurfaceBorderThickness remove os contornos permanentes e os restaura em alto contraste. Foco de teclado continua explícito, e seleção permanece indicada pelo fundo e pelos marcadores existentes. Campos de entrada e desenhos musicais preservam seus indicadores funcionais.

O menu de reticências alinha a borda direita da superfície ao botão, descontando a margem de sombra do template WPF UI 4.3 e a transformação de escala/DPI. Submenus abrem para a esquerda, em direção ao conteúdo. O contrato dos templates usados (Border no ContextMenu e Popup no MenuItem) é validado pelos testes do menu, incluindo limites da janela a 640, 920 e 1920 DIPs.

A pedido visual posterior, a leitura do inventário usa a animação indeterminada nativa do WPF UI, conservando a contagem real de cabinets no texto e na descrição acessível. Esta apresentação substitui a barra preenchida 0–8; transferências de IR continuam mostrando progresso determinado por bytes.

### Seleção e foco sem outline — 18/09/2026

Esta regra substitui as exceções anteriores de contorno para foco e alto contraste: seleção e foco usam preenchimento com as cores do tema, sem anéis ou bordas de destaque. Os templates nativos recebem recursos de borda transparentes e um indicador de foco por preenchimento. Presets, navegação, tiles, microfone e knobs usam o mesmo princípio, preservando os desenhos funcionais de valor dos controles musicais. A geometria e as interações permanecem preservadas.

### Galeria: categorias visíveis e ações compactas

O filtro de amplificadores usa ListBox/ListBoxItem nativos WPF UI em faixa horizontal com quebra: Todos, Clean, Overdrive e Distortion. As categorias continuam selecionáveis pelo teclado, sem popup adicional. Na galeria de cabinets o filtro permanece oculto. Os botões do ContentDialog mantêm comandos e templates da biblioteca, mas suas colunas usam largura automática, mínimo de 120 DIPs e alinhamento à direita. A adaptação é local à galeria e verificada contra o template WPF UI 4.3.0.

### Cores semânticas — 3.3.100

Os aliases de texto e atenção são atualizados pelo serviço de tema a partir dos recursos nativos WPF UI. Preservar as superfícies neutras e as cores de categoria dos efeitos. Textos e ícones selecionados herdam o foreground do item, inclusive a contagem de IRs. No alto contraste, textos, seleção e contornos usam as cores do Windows; cards recebem contorno de 1 DIP. Nos temas claro e escuro, manter a apresentação sem contornos permanentes. Os estados de texto dos botões seguem o template nativo.
