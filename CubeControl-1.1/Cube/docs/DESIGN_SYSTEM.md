# Design system do cubeControl

[Documentação](README.md) · Revisão: 16/10/2026 · [Decisões históricas](historico/historico-interface.md)

Referência para manutenção da interface WPF/.NET 10 com WPF UI 4.3.0. Esta revisão separa o comportamento atual das decisões antigas, que incluíam regras posteriormente substituídas. Critérios de acessibilidade são metas de verificação, não uma declaração de conformidade já comprovada.

## Referência aprovada e aplicação ao restante do app

A seção **Pedaleira**, incluindo o seletor de presets, a cadeia de efeitos e o editor do módulo selecionado, é a referência visual e de interação aprovada para todo o aplicativo. As adequações futuras de IRs, Dispositivo e ajuda, Backing Tracks e Criador de amps devem seguir esta base. A existência de um estilo particular em outra seção não o torna um padrão global.

O padrão combina superfícies neutras, hierarquia por preenchimento e espaçamento, tipografia Segoe UI, comandos WPF UI e controles musicais com artes e acabamentos próprios. Cores de efeitos são acentos de identificação. O conteúdo e a organização funcional de cada seção podem variar; cores, densidade, estados e componentes equivalentes devem permanecer consistentes com a Pedaleira.

Esta revisão incorpora a consolidação dos recursos compartilhados de aparência. A Pedaleira continua sendo a referência de composição e interação; os dicionários compartilhados definem os recursos usados em todo o app. Esta atualização documental confere regras contra o código, sem constituir nova homologação visual ou declaração de acessibilidade integral.

## Tokens e medidas da referência

### Organização dos recursos

O [App.xaml](../windows/Cube.Desktop/App.xaml) carrega `ThemesDictionary` e `ControlsDictionary` do WPF UI e, em seguida, estes dicionários, nesta ordem:

1. [Colors.xaml](../windows/Cube.Desktop/Appearance/Resources/Colors.xaml): mapeamentos semânticos `CubePalette.*` e paletas funcionais claras/escuras.
2. [Typography.xaml](../windows/Cube.Desktop/Appearance/Resources/Typography.xaml): famílias, tamanhos, entrelinhas e estilos de texto.
3. [Icons.xaml](../windows/Cube.Desktop/Appearance/Resources/Icons.xaml): identificadores `CubeIcon*`, tamanhos e estilos dos ícones Fluent.
4. [Styles.xaml](../windows/Cube.Desktop/Appearance/Resources/Styles.xaml): estilos de componentes e templates compartilhados, incluindo controles musicais. Carrega também os templates adaptados em `Appearance/Resources/Borderless/`, derivados do WPF UI 4.3.0, com chaves próprias `Cube*`, preservando as partes de interação da biblioteca.

Os arquivos `colors.xaml`, `typography.xaml` e `icons.xaml` na raiz do repositório são páginas demonstrativas da galeria WPF UI. Não são dicionários carregados pelo aplicativo nem devem ser importados diretamente.

Reutilizar as chaves existentes, sem copiar valores para estilos locais. Recursos dependentes do tema e medidas compartilhadas usam `DynamicResource` em XAML e `SetResourceReference` em elementos criados por C#. Estilos e identificadores imutáveis podem usar `StaticResource`. Verificar valores locais, que têm precedência sobre setters dos estilos, antes de criar ou alterar um recurso global.

### Cores e temas

| Recurso do app | Recurso WPF UI em claro/escuro | Uso |
| --- | --- | --- |
| `CubeCanvas` | `ApplicationBackgroundBrush` | Fundo sólido da aplicação |
| `CubeCard` | `CardBackgroundFillColorDefaultBrush` | Cards e seletores em repouso |
| `CubeInset` | `LayerFillColorDefaultBrush` | área de equipamento e hover dos tiles |
| `CubeWell` | `ControlFillColorInputActiveBrush` | áreas rebaixadas |
| `CubeFloatingSurface` | `SolidBackgroundFillColorSecondaryBrush` | Superfícies flutuantes |
| `CubeSelected` | `CubeSelectedBrush` (paleta por tema) | Seleção e expansão |
| `CubeSelectedText` | `TextFillColorPrimaryBrush` | Conteúdo selecionado |
| `CubeBorder` | `ControlStrokeColorDefaultBrush` | Contornos previstos pelo componente |
| `CubeAccent` / `CubeFocus` | `AccentFillColorDefaultBrush` | Destaque e foco do sistema |
| `CubeText` | `TextFillColorPrimaryBrush` | Texto principal |
| `Muted` | `TextFillColorSecondaryBrush` | Texto secundário |
| `CubeDisabledText` | `TextFillColorDisabledBrush` | Texto desabilitado |
| `CubeWarning` | `SystemFillColorCautionBrush` | Atenção |

`ThemeService` resolve os mapeamentos `CubePalette.*` após a troca de tema, coordena Mica e aplica os acabamentos. `CubeAccent` e `CubeFocus` acompanham o destaque do sistema; não são uma paleta cinza fixa nem substituem as cores funcionais dos módulos. Não sobrescrever globalmente recursos nativos por nome ou por conterem `BorderBrush`; preservar os estados e templates do WPF UI.

`CubeSurfaceBorderThickness` vale 1 DIP nos três tratamentos de aparência. Cada template determina onde utilizar esse recurso; isso não obriga todos os componentes a terem contorno. Em alto contraste, os recursos próprios usam `SystemColors`: superfícies em `WindowBrush`, texto e contornos em `WindowTextBrush`, seleção/foco em `HighlightBrush`, texto selecionado em `HighlightTextBrush` e texto desabilitado em `GrayTextBrush`.

As paletas funcionais `Backing*`, dos gráficos e dos knobs permanecem em `Colors.xaml`; `ControlArtwork` pode aplicar cores e artes específicas do acabamento escolhido. Brushes de superfície do WPF UI podem ser translúcidos: avaliar contraste sobre o fundo efetivamente composto, não somente pelos canais RGB isolados.

### Contraste de seleção e repouso

A referência aprovada em 19/10/2026 é o contraste entre o cabeçalho destacado de **Atalhos no pedal** e os cabeçalhos em repouso na imagem de Dispositivo e ajuda. Os valores escuros abaixo são a aproximação normativa dessa referência. Aplicar somente aos estados e interações que já existem: esta regra não torna cards, botões de ação ou outros elementos selecionáveis.

| Estado / recurso | Escuro | Claro |
| --- | --- | --- |
| Repouso — `CubeItemRest` | `#292929` | `#FAFAFA` |
| Hover — `CubeItemHover` | `#3A3A3A` | `#E8E8E8` |
| Seleção — `CubeSelected` | `#555555` | `#D0D0D0` |

Usar fundos opacos, independentes da superfície hospedeira, em navegação, presets, cadeia, listas, árvores, abas e filtros. Listas suspensas e menus seguem a especialização abaixo. Seleção prevalece sobre hover e permanece após retirar o mouse ou perder o foco. Nos expansores existentes, o estado expandido usa o preenchimento selecionado somente no cabeçalho; o conteúdo conserva sua superfície estrutural. Hover responde apenas ao cabeçalho.

Foco sozinho não representa seleção nem colore todo um painel quando um campo interno recebe foco. Preservar seus indicadores próprios, os marcadores de seleção e as cores funcionais dos controles musicais, efeitos, mute/solo e medidores. Itens desabilitados usam o fundo de repouso e texto desabilitado, sem destaque de hover/seleção. Alto contraste usa as cores do sistema, incluindo `HighlightTextBrush` no cabeçalho expandido.

Consumidores usam `DynamicResource` em XAML ou referências de recursos em C#, sem paletas locais concorrentes. Esta alteração foi limitada a aparência e documentação, sem testes, compilação ou homologação visual; a comparação renderizada permanece pendente.

### Listas suspensas e menus

A referência aprovada é a imagem da lista de fusos horários enviada em 19/10/2026: painel contínuo, itens integrados ao fundo, destaque discreto e marcador lateral na opção escolhida. Aplicar aos ComboBox, menus de ações, menus de contexto, preferências e submenus, sem tornar ações comuns selecionáveis. As cores são aproximações normativas da imagem, sem textura ou transparência obrigatória.

| Recurso | Escuro | Claro |
| --- | --- | --- |
| `CubePopupSurface` | `#292929` | `#FAFAFA` |
| `CubePopupHover` | `#3A3A3A` | `#E8E8E8` |
| `CubePopupSelected` | `#383838` | `#E4E4E4` |
| `CubePopupBorder` | `#404040` | `#D6D6D6` |

O painel usa `CubePopupCornerRadius` de 8 DIPs e `CubePopupBorderThickness` de 1 DIP, exceções explícitas ao raio geral de 4 DIPs e às superfícies sem contorno. A sombra suave usa `CubePopupShadowOpacity`. Itens usam fundo transparente em repouso, raio de 4 DIPs, `CubePopupItemMinHeight` de 40 DIPs, margem `CubePopupItemMargin` de 6,2,6,2 (4 DIPs entre itens) e padding `CubePopupItemPadding` de 10,8,10,8. Texto usa a família do app e tamanho de corpo.

Seleção prevalece sobre hover; desabilitação prevalece sobre ambos, com fundo transparente e `CubeDisabledText`. O marcador de seleção acompanha o destaque do sistema. Foco, hover e seleção continuam estados distintos. Preservar indicadores de opções marcadas, ícones, atalhos, grupos, amostras de cor e campos incorporados aos menus. Manter rolagem vertical, visibilidade da opção escolhida ao abrir, teclado, Escape e interação dos submenus.

A aparência do popup é independente do campo fechado: seletores compactos e mostradores musicais conservam suas dimensões, cores e interação. Estilos locais de itens devem herdar os estilos compartilhados. A paleta geral de seleção permanece válida fora desses popups.

`ThemeService` atualiza os recursos dinamicamente. Em alto contraste, painel usa `WindowBrush`, contorno usa `WindowTextBrush`, hover/seleção usam `HighlightBrush` e seus textos usam `HighlightTextBrush`; a sombra tem opacidade zero. Não substituir recursos globais da biblioteca.

Esta implementação recebeu somente revisão estática, sem testes ou compilação. Permanecem pendentes de conferência visual: lista longa, seleção com hover em outra opção, item desabilitado, submenu, agrupamentos, temas claro/escuro/alto contraste e janela compacta.

### Estados e abertura dos ComboBox

As imagens de seleção de calendários adicionais aprovadas em 19/10/2026 complementam a referência de listas suspensas especificamente para ComboBox. Menus conservam o padrão anterior.

| Estado | Aparência |
| --- | --- |
| Campo fechado | Texto e chevron centralizados verticalmente, raio de 4 DIPs, sem marcador lateral |
| Campo em hover | Clareamento discreto, mantendo o foco acessível independente |
| Opção em repouso | Fundo transparente, integrado ao painel |
| Opção em hover ou navegação por teclado | `CubePopupHover`, sem criar seleção |
| Opção selecionada, com ou sem hover | `CubePopupSelected` e marcador lateral de 3 × 20 DIPs |
| Opção desabilitada | Fundo transparente, `CubeDisabledText`, sem marcador |

O campo usa `CubeComboField` (`#333333` escuro / `#FAFAFA` claro) e `CubeComboFieldHover` (`#3A3A3A` / `#E8E8E8`). Alturas compactas e fundos musicais explícitos permanecem autoritativos, inclusive no hover. Os recursos acompanham o tema; em alto contraste, o campo usa `WindowBrush`. O popup mantém os recursos `CubePopup*`, independentemente do acabamento do campo.

`CubeComboSelectionMarker` depende somente de seleção e habilitação: foco não o mostra nem altera sua altura. A seleção permanece visível quando outro item recebe hover. Desabilitação prevalece sobre seleção e hover; as cores do sistema e o foco acessível permanecem ativos.

`ComboBoxPopupPlacement.Enabled`, aplicado pelo estilo compartilhado e encaminhado ao popup, ativa a abertura sobreposta. Na abertura, trazer a opção escolhida à área visível e alinhar o centro da linha ao centro do campo, alinhando o texto horizontalmente quando houver espaço. Sem seleção, alinhar a primeira linha sem selecioná-la; lista vazia mantém a altura mínima. Considerar margens, sombra, padding, zoom, DPI e área útil do monitor. Nas bordas, a visibilidade prevalece sobre o alinhamento exato; listas longas permitem rolagem vertical.

O posicionamento estabiliza na abertura: rolagem e navegação posteriores não fazem o popup acompanhar cada linha. A transição do painel mantém opacidade, sem deslocamento vertical que desalinhe a opção. Preservar confirmação, cancelamento, Escape, clique externo, edição, agrupamentos, conteúdos personalizados e bindings. Descarregar ou substituir o template cancela trabalho pendente e libera os eventos do popup.

Verificação desta alteração limitada à revisão estática e ao diff, sem testes nem compilação. Validação visual pendente: campo fechado em repouso/hover/foco, seleção com hover próprio ou em outra opção, teclado, lista longa/agrupada, ausência de seleção, lista vazia, reabertura, bordas da tela, zoom, DPI e temas claro/escuro/alto contraste.

### Arredondamento de superf?cies e controles

Exceção: os painéis de listas suspensas, menus e submenus usam o raio de 8 DIPs definido acima; seus itens continuam com 4 DIPs.

Usar `CubeCardCornerRadius` (4 DIPs) para cards, pain?is de agrupamento, expansores e superf?cies externas de di?logos, menus, tooltips e popups. Usar `CubeControlCornerRadius` (4 DIPs) para bot?es comuns, campos, buscas, filtros, abas, seletores, itens de menu e tiles, incluindo presets. As buscas seguem o mesmo raio dos campos, sem formato de c?psula. Cards de slots de packs usam 4; linhas compactas de m?sicas e resultados usam 4 por serem itens internos.

Os recursos ficam em `Appearance/Resources/Colors.xaml`, carregado antes dos estilos. Reutilizar `DynamicResource` em XAML e `SetResourceReference` em C#. As variantes `CubeCardLeftCornerRadius`, `CubeCardTopCornerRadius` e `CubeCardBottomCornerRadius` preservam as jun??es retas entre partes de uma superf?cie; expansores dividem o raio compartilhado entre cabe?alho e conte?do. Camadas de foco, hover e sombra acompanham a geometria da superf?cie correspondente.

Esta padroniza??o altera somente o arredondamento: n?o remove contornos nem modifica espessura, cor, preenchimento, dimens?es, espa?amento ou regras de foco e sele??o. Preservar artes e formas musicais de knobs, faders, switches, footswitches e transporte, al?m dos marcadores laterais, indicadores circulares e desenhos dos gr?ficos. O chrome nativo de janelas e o fallback de popups sem transpar?ncia conservam as limita??es da plataforma.

### Tipografia e componentes

| Estilo/recurso | Especificação | Aplicação |
| --- | --- | --- |
| `CubeUiFont` | Segoe UI Variable Text → Segoe UI Variable → Segoe UI | Família da interface |
| `TitleText` | 28, SemiBold, entrelinha 36 | Título da página |
| `TitleLargeText` | 40, SemiBold, entrelinha 52 | Título grande quando necessário |
| `DisplayText` | 68, SemiBold, entrelinha 92 | Destaque excepcional; não obrigatório nas páginas |
| `SectionTitle` | 20, SemiBold, entrelinha 28 | Título de seção quando necessário |
| `BodyText` / `BodyStrongText` | 14, entrelinha 20; Normal / SemiBold | Corpo, nomes e títulos compactos |
| `CaptionText` / `HelpText` | 12, entrelinha 16; ajuda em `Muted`, com quebra | Estado e orientação secundária |
| `Card` | Raio 4, padding 24 | Superfície principal; faixa de presets usa padding 24,16 |
| `SelectionTile` | Raio 4, padding interno 12, margem 0,0,8,8 | Seletores da cadeia e equivalentes |
| `PresetTile` | Raio 4, altura 116, margem horizontal 18 | Especialização do seletor A/B/C |
| `CubeButtonStyle` | Altura mínima 40, padding 16,8, texto 14 | Comandos; preservar templates e estados WPF UI |
| `CubeIconButtonStyle` | 40 × 40, padding 0 | Comandos somente com ícone e nome acessível |
| `SettingsAction` | Altura mínima 64, padding 16,12 | Ações em cards |
| `QuickAction` | Altura mínima 64, padding 12,10, raio 4, fundo transparente | Ações contextuais com ícone, título e ajuda |

Todas as medidas são DIPs antes da escala de interface. Para compor novas seções, reutilizar os intervalos da referência: 4–8 entre elementos próximos, 12–16 entre grupos e 24 no interior de superfícies principais. A Pedaleira usa 6 entre título e descrição e 12 após a descrição. Não criar uma escala tipográfica ou densidade própria por página. Ícones seguem os recursos descritos abaixo.

Os tamanhos usam `CubeFontSizeCaption`, `CubeFontSizeBody`, `CubeFontSizeSubtitle`, `CubeFontSizeTitle`, `CubeFontSizeTitleLarge` e `CubeFontSizeDisplay`; as entrelinhas correspondentes usam o prefixo `CubeLineHeight`. `BodyStrongText` compartilha as medidas de Body. `SectionTitle` corresponde a Subtitle. `CubeUiFont` e `ContentControlThemeFontFamily` mantêm os fallbacks da família. Preservar Consolas em trechos de código e mostradores monoespaçados, além da tipografia funcional desenhada em gráficos e artes.

### Ícones e identidade dos controles

| Papel | Recurso de tamanho | Estilo | DIPs |
| --- | --- | --- | --- |
| Ação compacta | `CubeIconSizeAction` | `CubeActionIcon` | 16 |
| Navegação | `CubeIconSizeNavigation` | `CubeNavigationIcon` | 20 |
| Destaque | `CubeIconSizeEmphasis` | `CubeEmphasisIcon` | 24 |

`NavigationIcon` especializa navegação com margem e estados; `QuickActionIcon` usa o tamanho de ação. Reutilizar os identificadores `CubeIcon*` em XAML. Em C#, [AppIcons.Create](../windows/Cube.Desktop/Appearance/AppIcons.cs) cria uma instância de `SymbolIcon` por uso, com referências aos recursos; o parâmetro `navigation` seleciona o papel de navegação. Para outro papel, aplicar o estilo compartilhado correspondente à instância. Não compartilhar uma instância visual entre pais diferentes.

Fluent padroniza ações e navegação. Preservar marca, fotos de equipamentos, knobs, faders, switches, footswitches e transporte com arte própria. [ControlArtwork](../windows/Cube.Desktop/Appearance/ControlArtwork.cs) e [ControlFinishCatalog](../windows/Cube.Desktop/Appearance/ControlFinishCatalog.cs) administram acabamentos e alternativas de alto contraste; uma revisão visual não deve fixar o acabamento oficial nem remover a personalização.

## Composição e interação da Pedaleira

A leitura segue título e orientação, presets/ações, cadeia, avisos contextuais e editor selecionado. Adotar essa hierarquia nas outras seções: contexto primeiro, escolha do objeto, ações e conteúdo de edição. Separar ações de recuperação das de importação/exportação e manter a ação principal identificável.

O editor combina uma área de equipamento em `CubeInset` com imagem, nome e ações contextuais e uma área de parâmetros com margem 24. O `AdaptivePanel` usa breakpoint 740 e participação inicial de 40% para o equipamento; no editor avançado, 1040 e 30%. A faixa de ações dos presets usa breakpoint 760. Esses valores são referências dos componentes atuais, não larguras universais para todas as páginas. Reorganizar grupos conforme o espaço disponível, preservando ordem de leitura e rolagem vertical da página.

### Cadeia de efeitos

- Ordem visual fixa: **Amp → Chorus → Phaser → Delay → Reverb → Cabinet**. Essa apresentação não é uma declaração de roteamento interno medido nem oferece reordenação por arraste.
- Cada tile apresenta número, switch, nome e estado. Usar `SelectionTile`, `BodyStrongText` no nome e `CaptionText` no estado; texto de estado truncado deve continuar disponível no tooltip.
- Selecionar o tile escolhe o editor. Acionar o switch altera a função indicada pelo seu rótulo; seleção e ativação são estados independentes. No Amp, o switch representa Tone, não bypass geral. Chorus e Phaser compartilham o parâmetro de modulação.
- Repouso usa `CubeCard`; hover usa `CubeInset`; seleção usa `CubeSelected` e `CubeSelectedText`; indisponibilidade usa `CubeDisabledText`. A seleção tem precedência sobre hover e o estado desabilitado sobre ambos. Preservar também a legibilidade do número do módulo nesses estados.
- A roda sobre a cadeia navega entre módulos. O layout atual de `StagePanel` distribui tiles de mesma largura em 6 colunas a partir de 1260 DIPs disponíveis, 3 a partir de 630, 2 a partir de 420 e 1 abaixo disso. A sequência continua nas linhas seguintes, mantendo a ordem de leitura. Não forçar seis cards comprimidos em uma janela estreita.

| Módulo | Acento escuro | Acento claro |
| --- | --- | --- |
| Amp | `#D5AD7D` | `#82551F` |
| Chorus | `#80B8A8` | `#226E59` |
| Phaser | `#D9A071` | `#965622` |
| Delay | `#82ACD2` | `#2D638B` |
| Reverb | `#B49ACF` | `#765394` |
| Cabinet | `#C5B77F` | `#726021` |

Resolver essas cores pelo modelo e por `ThemeService.ModuleColor`, incluindo o tratamento de alto contraste. Usá-las em identificadores, detalhes e controles do módulo, sem transformar toda a superfície em uma cor de categoria nem comunicar ligado/desligado apenas pela cor.

### Controles musicais e imagens

Reutilizar `Knob`, `GearImageButton`, `GearArt` e os templates oficiais de switches e faders. Os knobs do `DialTemplate` têm 94 × 94 DIPs, célula de largura 132 e rótulo acima; Master usa 104 × 104. Os valores ficam em tooltip e automação, sem campos numéricos permanentes adicionais. Os grupos de knobs usam `WrapPanel`.

O switch compartilhado tem altura mínima 32 e face de 56 × 26. Switches, faders, knobs e footswitches usam os recursos do acabamento selecionado, incluindo as artes oficiais e as alternativas de alto contraste. Esses elementos representam operações diferentes e não devem ser trocados entre si apenas por semelhança visual.

Knobs e faders são controles de ajuste, não itens selecionáveis: não apresentar estado de seleção, preenchimento de seleção nem listra indicadora à esquerda ao clicar, arrastar ou receber foco. Esta regra inclui os faders desenhados pelo `Knob`, os sliders e o `BackingFader`. Preservar o foco funcional e os atalhos de teclado; somente a navegação por teclado apresenta o contorno discreto compartilhado `CubeParameterFocusVisualStyle`, com `CubeFocus`, sem marcador lateral. O foco de teclado não representa seleção do parâmetro. Preservar nomes e valores acessíveis, ajustes por mouse e alternativas de alto contraste.

Equipamentos preservam proporção, transparência e cores da imagem. A área de arte da referência tem altura 240; não aplicar essa dimensão como obrigação para qualquer imagem do app. Quando há galeria, a imagem oferece clique para abrir a seleção e roda para trocar equipamento, com indicação e tooltip próprios. Personalização de imagem fica em um expansor secundário e informa que não altera o som.

A imagem principal do gabinete e as demais imagens exibidas por `GearImageButton` não são itens selecionáveis: não apresentar faixa lateral nem preenchimento de seleção. Preservar clique, roda, menu de personalização e acesso por teclado. O foco de navegação por teclado usa apenas o contorno discreto de `CubeParameterFocusVisualStyle`; não representa seleção da imagem. Esta regra não altera a indicação do item escolhido dentro das galerias.

As ilustrações de IRs devem respeitar quantidade e disposição dos falantes documentadas: não representar 2×12 com um falante, nem inferir 4×12 apenas pelo nome Rectifier. Reutilizar uma arte por família; microfone, distância e equalização não exigem nova imagem. Preservar o estilo da família e distinguir identificação confirmada, provável e desconhecida. Configuração conhecida sem modelo usa arte sem logotipo compatível; configuração desconhecida usa frente opaca sem cones aparentes. As artes são ilustrativas, não prova acústica ou identificação por conteúdo. Imagens personalizadas mantêm precedência. Fontes, inventário e prompts ficam na [auditoria das imagens de IRs](referencias/ir-artwork-audit/README.md).

Diagramas de efeito acompanham os ajustes e são ilustrativos. Preservar a distinção entre o editor simples com knobs e os criadores gráficos com manipulação direta, descritos ao final deste documento.

## Princípios

1. Mostrar com clareza preset em edição, efeito ligado, prévia, envio e gravação.
2. Usar nomes, limites e consequências sustentados pelo código/protocolo.
3. Manter componentes e recursos compartilhados antes de criar novos estilos.
4. Separar controles de áudio, personalização visual e diagnóstico.
5. Preservar operação por teclado, nomes acessíveis, foco e localização.
6. Distinguir testes simulados, inspeção visual e validação com hardware.

## Estrutura atual

A navegação reúne **Pedaleira**, **IRs**, **Dispositivo e ajuda**, **Backing Tracks** e **Criador de amps**. O [histórico](historico/historico-interface.md) descreve fases com apenas três áreas; essa contagem não representa o produto atual.

A/B/C escolhem o preset em edição, sem confirmar o footswitch ativo. A cadeia visual apresenta Amp, Chorus, Phaser, Delay, Reverb e Cabinet. Chorus/Phaser compartilham o parâmetro de modulação. Selecionar um card abre o editor; o switch é uma ação distinta. No Amp, Tone não significa bypass geral.

A navegação superior usa seletores locais; não há integração de páginas pelo `NavigationService`. Tema, idioma, zoom e detalhes de conexão ficam no menu de reticências. A conexão continua visível no cabeçalho. Backing Tracks oferece um miniplayer durante a reprodução carregada.

## Aparência, escala e layout

- Usar recursos semânticos por `DynamicResource`. `ThemeService` integra Claro, Escuro, Sistema e alto contraste.
- Superfícies escuras usam cinzas neutros; cores de módulo identificam categorias e não substituem mensagens de estado.
- Textos/atenção usam recursos WPF UI; o destaque do Windows não deve virar uma cor fixa da marca.
- Fotos preservam proporção e cores. Imagens personalizadas têm prioridade sobre padrão e fallback genérico.
- Mica acompanha a preferência de transparência do Windows e tem fundo sólido de fallback.
- Medidas WPF são DIPs. O zoom de 50–200% atua sobre a densidade base de 90%; 100% corresponde ao layout base, sem mudar o DPI do Windows.
- Cabeçalho, faixas e diálogos devem reorganizar comandos em larguras pequenas. Use os painéis adaptativos existentes e mantenha ações acessíveis por rolagem.
- A cadeia mantém a ordem dos módulos e adapta o número de colunas conforme `StagePanel`; a roda sobre ela navega entre módulos. A descrição anterior de faixa obrigatoriamente horizontal foi substituída pela implementação atual, detalhada acima.

Superfícies claras/escuras combinam preenchimento, espaçamento e contornos definidos pelos componentes e recursos compartilhados. O tratamento de alto contraste segue os recursos do sistema. As antigas regras de eliminar contornos globais ou manter o foco sempre cinza não se aplicam. Foco precisa permanecer identificável nos controles nativos e musicais.

Detalhes de zoom, rolagem e duração das animações: [Interação no Windows](desenvolvimento/windows-interaction.md). Os callbacks de renderização não garantem FPS apresentados pelo monitor.

### Transições de entrada das seções

O padr?o de transi??o de conte?do do aplicativo ? opacidade de zero ao valor original com deslocamento vertical de 8 a 0 DIPs, em 167 ms, com `CubicEase/EaseOut`. Aplicar tamb?m a cards da cadeia, troca de presets, menus, listas suspensas, tooltips, expansores e di?logos. Usar uma ?nica superf?cie por troca, evitando anima??es sobrepostas entre pais e filhos. Feedbacks funcionais de hover, arraste e rolagem n?o s?o transi??es de conte?do.

Usar o comportamento compartilhado [SurfaceMotion](../windows/Cube.Desktop/Controls/SurfaceMotion.cs) no contêiner principal de cada seção: `EditorScroll` (Pedaleira), `IrScroll` (IRs), `BackingPage` (Backing Tracks), `AmpEditorHost` (Criador de amps) e `HelpScroll` (Dispositivo e ajuda). Habilitar `SurfaceMotion.Enabled` e definir `SurfaceMotion.Offset="8"`: a entrada combina opacidade de zero até o valor original e deslocamento vertical de 8 até 0 DIPs, em 167 ms, com `CubicEase/EaseOut`. Não duplicar storyboards por página nem animar cada controle individualmente para produzir essa entrada.

A transição ocorre no carregamento e quando a seção volta a ficar visível. Em `AmpEditorHost`, vincular `SurfaceMotion.Key` ao próprio `Content` para reproduzir a entrada quando o convite inicial é substituído pelo editor. Atualizações de valores e de áudio não devem disparar novamente a entrada da seção.

O movimento é apenas de apresentação, sem alterar layout, foco, comandos ou processamento de áudio. O comportamento compartilhado cancela as animações ao ocultar ou descarregar a superfície e restaura sua opacidade e transformação originais. Respeitar a desativação das animações no Windows, alto contraste e renderização sem aceleração gráfica: nessas condições, exibir o conteúdo diretamente. `DesiredFrameRate=240` é uma meta de agendamento, não uma garantia de FPS apresentados pelo monitor.

### Campos de busca e filtros

Campos de busca e filtros não são itens selecionáveis de navegação e não devem apresentar faixa lateral nem preenchimento de seleção ao receber foco. Usar um contorno discreto de 1 DIP em `CubeFocus`, sem alterar as dimensões do conteúdo. Aplicar esta regra aos templates compartilhados de busca (`CubeSearchTextBoxStyle` e `CubeSearchAutoSuggestBoxStyle`), aos campos ComboBox e às especializações locais de filtros. O estado desabilitado não apresenta contorno de foco.

Foco para edição, seleção do texto e escolha de uma opção são estados distintos. Preservar digitação, seleção de texto para copiar/editar, limpeza, sugestões e atalhos. As opções escolhidas dentro das listas suspensas e os filtros ativados continuam identificáveis; nos filtros por categoria, manter o preenchimento do filtro ativo, sem faixa lateral, e usar somente contorno para foco. Esta regra não remove os marcadores de seleção de abas, resultados, listas ou navegação.

## Componentes

| Componente | Regra atual |
| --- | --- |
| Botões | Reutilizar `CubeButtonStyle` e templates WPF UI; texto indica a ação |
| Seletores de equipamento | Imagem abre galeria; destaque preliminar, aplicar/clique duplo confirma, cancelamento não aplica |
| Knobs da Pedaleira | Valor por tooltip/automação, sem restabelecer campos numéricos permanentes das versões antigas |
| Controles do criador de amps | Faders/knobs, edição numérica por Enter, referência visível e ajuste fino com Shift; duplo clique ou Backspace restaura o ajuste inicial, sem setas de retorno; respeitar faixas individuais |
| Microfone de IR | Aplica ao concluir o gesto; 0–100 não significa centímetros |
| Gráfico dos EQs | Representa os coeficientes do filtro, sem alegar medição acústica |
| Galeria e associações | Identidades estáveis; gabinete reconhecido pelo conteúdo, não pelo slot |
| Notificações | Snackbar para resultado breve; erro ou orientação importante permanece consultável no editor |
| Confirmações | Manter destino, consequência e backup definidos pelo fluxo; arquivo/slot não pode ficar ambíguo |

O criador possui limites específicos em cinco grupos de coeficientes; o limite atual de ganho interno é 100 e aceita os nove amps originais; veja [Criador de amps](referencias/criador-de-amps.md). Não converta os nomes musicais dos grupos em bandas de frequência comprovadas. Os códigos A/B preservam a rastreabilidade. Resposta usa dois painéis com chave discreta, visor seletor e intensidade, reaproveitando PNGs vintage transparentes e cards WPF UI. Os 16 trechos internos ficam ocultos e limitados a 67–133%; os perfis são propostas de escuta, não uma curva de palhetada ou equivalência elétrica comprovada. Em alto contraste os knobs usam desenho vetorial e cores do sistema. A EQ distingue A/B por cor, letra e traço, com reset da banda. Estados locais, prévia RAM e gravação verificada ficam separados. A página usa recursos locais e uma única rolagem por modo de layout.

## Player, mixer e busca de backing tracks

A seção segue a composição Mini DAW aprovada: barra contínua superior de 68 DIPs; arranjo e mixer em proporção 59/41; biblioteca lateral de 310 DIPs, sobreposta abaixo de 1200 DIPs de largura disponível. A navegação principal permanece. Não usar cartões, badges ou expansores para fragmentar o arranjo. Janelas menores usam rolagem horizontal na barra e nos canais; o mixer permanece visível, sem redução de controles por escala.

A barra apresenta dispositivo WASAPI, REC, Play/Pause, Stop, Loop, metrônomo, BPM, velocidade, tom, Cortar, Importar e Exportar. Reutilizar superfícies, tipografia, ícones e controles musicais compartilhados. Os cabeçalhos têm 335 DIPs; as pistas, 96 DIPs. Régua em compassos, clipes, loop e cursor compartilham o eixo temporal.

Cabeçalhos e mixer apresentam S/M/R/I, entrada, ganho e pan sincronizados. I monitora a entrada independentemente de R, sem iniciar transporte ou gravação. O estado efetivo é desligado em falha de dispositivo e ao restaurar sessões. Bloqueios de captura e roteamento continuam válidos. Não criar canais auxiliares de monitoração, Bus ou metrônomo. Master fica fixo à direita do mixer, sem monitoração; os canais reais rolam horizontalmente e usam ganho em dB e medidores compartilhados `BackingGainControl` e `BackingFaderMeter`.

A biblioteca contém Arquivos locais e YouTube. Arquivos locais têm busca, categorias manuais Todos/Loops/One Shots/Vocais, prévia e inserção na pista/cursor; não mostrar conteúdo fictício. A prévia pausa a sessão quando pronta e não a retoma automaticamente. Pastas e categorias são persistidas separadamente dos arquivos de áudio.

Novas sessões têm uma pista vazia; a última sessão disponível é restaurada sem tocar, armar ou monitorar. Sessões legadas mantêm seus dados e processamento, incluindo tomadas e fades, embora ferramentas avançadas não façam parte da interface. Cortar divide no cursor, e operações essenciais oferecem desfazer/refazer. A gravação continua preservando áudio anterior e arquivos recuperáveis. A atualização visual nominal de 50 ms não comprova latência física. Persistência de sessões permanece na versão 4.

### Feedback de arrastar e soltar

Todos os fluxos de drag and drop usam uma camada visual compartilhada (`DragFeedback`), independente das bordas de seleção. O feedback contém a identidade do item arrastado e a ação prevista; não depende somente de cor ou do cursor do Windows.

- **Reordenar:** linha de inserção antes/depois do destino, com indicação textual e rolagem automática nas bordas. A ordem visual não altera o tempo das pistas.
- **Adicionar:** destacar a área receptora e identificar se o destino é a playlist, a sessão ou um slot vazio.
- **Substituir:** destacar o item afetado e informar origem e destino antes de soltar.
- **Trocar:** identificar os dois slots e a permuta de conteúdo; não apresentar a operação como simples substituição.
- **Inválido:** informar destino inválido e rejeitar a operação. Cancelamento, saída do destino e conclusão removem o feedback.

Usar `CubeAccent` para destinos válidos, `CubeDisabledText` para inválidos, `CubeFloatingSurface` e `CubeText` para o rótulo; esses recursos acompanham os temas e o alto contraste. O rótulo acompanha a posição de arraste dentro do destino. Não interceptar o mouse na camada visual. A prévia e o drop compartilham a avaliação de destino; soltar fora dele não modifica dados. Manter alternativas por botões, menus e teclado. No pack de IR, o drop altera somente o rascunho; aplicar ao pedal continua sendo uma ação separada.

## Progresso e mensagens

A operação central usa ring quando não existe medida e barra quando há progresso por bytes. O inventário mantém a contagem real no texto com apresentação indeterminada. Não apresente 100% como gravação concluída antes da conferência.

Mensagens devem informar o resultado, a pendência e a próxima ação disponível. Diferencie:

- Edição local, envio confirmado e gravação verificada.
- Prévia iniciada, confirmada e interrompida.
- Preset editado e preset fisicamente audível.
- Referência visual de equipamento e conteúdo identificado.
- Resultado de leitura e retenção após desligamento.

Evite notificações por movimento de knob ou monitoramento automático. Eventos e durações: [Notificações](desenvolvimento/snackbars.md).

## Localização e acessibilidade

Português é a base das chaves; inglês fica no catálogo. Alterações visíveis devem revisar ambos. Preserve nomes próprios, números, extensões e unidades reais. Consulte [Localização](../windows/Cube.Desktop/Localization/README.md).

Na revisão de interface, conferir teclado, foco, leitores de tela, contraste, alto contraste, DPI, zoom, janela compacta e textos longos nos dois idiomas. Metas: contraste 4,5:1 para texto comum, 3:1 para texto grande e indicadores essenciais. A paleta declarada não substitui medição renderizada.

Testes WPF simulados não comprovam uso físico por toque, leitura por Narrator, áudio nem troca real entre monitores. Use os modos pertinentes do [guia de testes](../windows/README.md), registrando limites da verificação.

## Fontes e manutenção

### Critérios para adequar outras seções

Ao revisar uma seção, usar a Pedaleira como comparação e registrar os seguintes itens:

- [ ] Superfícies, textos e estados usam os recursos semânticos compartilhados e respondem a Claro, Escuro, Sistema e alto contraste.
- [ ] Títulos, corpo, ajuda, espaçamentos, raios e densidade seguem as tabelas deste documento.
- [ ] Botões, ícones, cards de ação e controles musicais reutilizam os componentes existentes; especializações têm motivo funcional documentado.
- [ ] Valores locais em XAML e C# não anulam indevidamente recursos compartilhados; ícones criados por código usam instâncias próprias e os mesmos papéis visuais.
- [ ] Artes, acabamentos e personalização continuam disponíveis; a troca visual preserva valores e não envia comandos MIDI.
- [ ] Seleção, ativação, hover, foco, indisponibilidade e processamento são distinguíveis e coerentes com a referência.
- [ ] A ordem de leitura é clara, ações continuam acessíveis na janela compacta e os grupos se reorganizam com zoom de 50–200% e diferentes DPIs.
- [ ] Imagens preservam proporção e diagramas não sugerem uma medição inexistente.
- [ ] Teclado, nomes acessíveis, foco e textos longos foram verificados em português e inglês; contraste foi medido quando aplicável.
- [ ] Edição local, prévia, envio e gravação mantêm suas consequências explícitas.

Registrar na adequação a seção alterada, os componentes reutilizados, eventuais exceções e a evidência de verificação (tema, tamanho de janela, zoom e idioma). Critérios não verificados ficam marcados como pendentes. Diferenças existentes em outras páginas são candidatas a adequação, não justificativa automática para mudar a referência aprovada.

[Carregamento](../windows/Cube.Desktop/App.xaml), [estilos](../windows/Cube.Desktop/Appearance/Resources/Styles.xaml), [janela](../windows/Cube.Desktop/MainWindow.xaml), [tema](../windows/Cube.Desktop/Appearance/ThemeService.cs), [densidade](../windows/Cube.Desktop/Appearance/UiDensity.cs), [zoom](../windows/Cube.Desktop/Appearance/ZoomService.cs) e [editor de amps](../windows/Cube.Desktop/Views/AmpEditor.xaml) são as fontes da implementação.

Para a referência da Pedaleira, consultar também [layout da cadeia](../windows/Cube.Desktop/Controls/StagePanel.cs), [modelo visual dos módulos](../windows/Cube.Desktop/ViewModels/EditorViewModel.Visual.cs), [knob](../windows/Cube.Desktop/Controls/Knob.cs) e [interações da janela](../windows/Cube.Desktop/MainWindow.xaml.cs). Se código e documento divergirem, conferir a implementação e registrar a decisão antes de propagar a diferença para outras seções.

Este documento define o padrão; a skill `cube-design-system` orienta sua aplicação e verificação, sem manter uma segunda cópia dos tokens. Registrar evidências de validação separadamente das regras: uma compilação bem-sucedida não comprova todos os critérios, e falhas de testes auxiliares de catálogo ou MIDI não redefinem o padrão visual.

Registre novas decisões neste guia e mantenha histórico separado. Não altere protocolo, dependências ou semântica de persistência como consequência implícita de uma mudança visual. O antigo relatório `wpf-ui-audit.md` citado no histórico não está incluído nesta árvore; não é uma verificação reproduzida nesta revisão.

## Criadores gráficos de efeitos

As linhas representativas dos parâmetros de Chorus, Phaser, Delay e Reverb usam a cor do respectivo efeito na Pedaleira, nunca a cor de destaque do tema. Os quatro editores vinculam `EffectGraphEditor.Accent` a `DataContext.Stages[1..4].Color` do `UserControl` pai, mantendo uma única fonte de cor e as notificações de troca de tema. As cores-base existentes são Chorus `#80B8A8`, Phaser `#D9A071`, Delay `#82ACD2` e Reverb `#B49ACF`; `ThemeService.ModuleColor` fornece as variantes claras e de alto contraste. Não duplicar esses valores nos XAML dos editores.

Aplicar a cor do efeito também aos pontos de manipulação e às linhas Original/Dry, preservando o tracejado do sinal direto. Eixos, grades e linhas auxiliares sem parâmetro permanecem neutros. Preservar espessuras de seleção, estados de interação e foco acessível; botões, campos e indicadores de foco continuam usando os recursos da interface. Em alto contraste, o gráfico mantém a substituição por `CubeText`, resolvido para a cor de texto do sistema. Essa padronização não altera parâmetros, escalas, áudio, prévia ou comunicação com o pedal.

A correção de cores recebeu revisão estática dos bindings, do desenho compartilhado e das notificações de tema, sem testes ou compilação. Permanece pendente a conferência visual dos quatro efeitos em claro, escuro e alto contraste, incluindo seleção e mudança da cor de destaque do sistema.

Chorus, Phaser, Delay e Reverb usam manipulação direta de contornos, sem sliders, faders ou knobs internos. Os comandos usam botões e ícones WPF UI; os detalhes numéricos aparecem junto ao elemento selecionado. Alvos de 36 DIPs, foco visível e UI Automation compartilham as propriedades do modelo. Desenhos são ilustrativos e não animados; não representam medições. Os controles normais dos pedais permanecem separados. Consulte [mapeamentos e limitações](referencias/editores-graficos-de-efeitos.md).

### Padrão compartilhado de VU

Todos os indicadores de nível de áudio usam `BackingFaderMeter`, inclusive a pré-escuta de IR, integrado ao `BackingFader` quando houver controle de volume. Usar `BackingMeterEnvelope` para a escala de −60 a 0 dBFS, queda de 24 dB/s e retenção de pico por um segundo. Não usar barras de progresso percentuais para representar áudio. Preservar a origem da medição: na pré-escuta, o sinal enviado à saída, não o retorno processado pelo pedal. Os segmentos usam `CubeAccent`, alerta a partir de −6 dBFS com `CubeWarning`, marcador de pico com `CubeText` e recursos do sistema em alto contraste. O aviso textual de saturação acompanha o medidor; o fader mantém interação e nome acessível. Barras de progresso de reprodução, carregamento e exportação continuam separadas.

Nos cards de packs de IR, usar o `Knob` compartilhado de 94 × 94 DIPs com rótulo Volume, passo de 1%, tooltip percentual e identificação acessível do slot; não exibir campo numérico editável. O gesto do knob não inicia arraste do card. Ajustes pertencem à montagem. A ação Aplicar usa uma confirmação textual com OK/Sair, sem repetir a grade de edição. Packs predefinidos e novos IRs inseridos começam em 80%; packs pessoais preservam valores salvos.

## Regressão automatizada da aparência

A suíte `borderless` carrega e aplica os templates em WPF e exercita seleção, foco, desabilitação, temas e componentes flutuantes. Complementa as suítes de tema, responsividade e usabilidade; não substitui inspeção visual. Execução e limites estáo na [matriz de cobertura](desenvolvimento/test-coverage.md).


## Arraste compartilhado

Os arrastes existentes do editor de IR, playlist e stems usam GongSolutions.WPF.DragDrop 4.0.0 com adaptadores internos. Iniciar somente ap?s o limiar do sistema, sem capturar gestos de bot?es, knobs, faders, campos e waveform. A pr?via tem opacidade de 75%, escala de 1,03 e sombra discreta; a origem de movimenta??es mant?m seu espa?o com opacidade reduzida. C?pias preservam a origem.

A indica??o de destino distingue inser??o nas listas, troca entre slots, adi??o, substitui??o e destino inv?lido. O feedback permanece separado da sele??o e do foco. A rolagem autom?tica atua nas bordas dos visores; destinos internos de stems consomem o drop para n?o importar novamente na playlist.

Confirmar os dados somente no drop. Listas preservam o c?lculo por identidade na cole??o original, mesmo com filtros. Slots mant?m n?meros e comandos fixos: somente os conte?dos trocam de posi??o. A acomoda??o usa 200 ms com desacelera??o c?bica e capturas visuais dos elementos vis?veis; n?o modifica transforms nem bindings de layout. Importa??es mant?m suas valida??es e recebem encaixe visual ap?s inclus?o.

Respeitar `SurfaceMotion.Allowed`, anima??es do sistema e alto contraste. Em alto contraste, dispensar sombra, transpar?ncia e movimento; manter indica??o por contorno e texto com cores sem?nticas. Cancelamento, drop externo, falha e descarregamento limpam o feedback e restauram a origem. As alternativas por teclado e as regras de persist?ncia permanecem existentes. Novas ?reas de arraste devem reutilizar essa infraestrutura, sem habilita??o global indiscriminada.
