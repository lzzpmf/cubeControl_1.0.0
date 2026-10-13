# Design system do cubeControl

[Documentação](README.md) · Revisão: 10/10/2026 · [Decisões históricas](historico/historico-interface.md)

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
4. [Styles.xaml](../windows/Cube.Desktop/Appearance/Resources/Styles.xaml): estilos de componentes e templates compartilhados, incluindo controles musicais.

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
| `CubeSelected` | `SubtleFillColorSecondaryBrush` | Seleção |
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
| `Card` | Raio 14, padding 24 | Superfície principal; faixa de presets usa padding 24,16 |
| `SelectionTile` | Raio 10, padding interno 12, margem 0,0,8,8 | Seletores da cadeia e equivalentes |
| `PresetTile` | Raio 8, altura 116, margem horizontal 18 | Especialização do seletor A/B/C |
| `CubeButtonStyle` | Altura mínima 40, padding 16,8, texto 14 | Comandos; preservar templates e estados WPF UI |
| `CubeIconButtonStyle` | 40 × 40, padding 0 | Comandos somente com ícone e nome acessível |
| `SettingsAction` | Altura mínima 64, padding 16,12 | Ações em cards |
| `QuickAction` | Altura mínima 64, padding 12,10, raio 6, fundo transparente | Ações contextuais com ícone, título e ajuda |

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

Equipamentos preservam proporção, transparência e cores da imagem. A área de arte da referência tem altura 240; não aplicar essa dimensão como obrigação para qualquer imagem do app. Quando há galeria, a imagem oferece clique para abrir a seleção e roda para trocar equipamento, com indicação e tooltip próprios. Personalização de imagem fica em um expansor secundário e informa que não altera o som.

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

O transporte conserva suas artes próprias, sem legendas permanentes nos saltos de cinco segundos; tooltips e nomes acessíveis descrevem as ações. O ícone vetorial `CubeMetronomeIcon` compartilha a grade de 24 unidades, tamanho de navegação e cor herdada dos controles vizinhos.

No mixer, usar um único layout com faders horizontais e um único mute por canal junto ao percentual. O botão comunica o estado e oferece a ação inversa no tooltip/nome acessível. O painel limita a largura a 620 DIPs e usa 8 DIPs entre grupos; monitoração e metrônomo ficam sempre disponíveis. Os medidores de pico digital ficam atrás dos faders, sem interceptar interação, com escala de −60 a 0 dBFS e retenção de pico por um segundo. Medir cada canal após volume e mute e manter o aviso de saturação da entrada separado. Preservar as artes dos faders, recursos semânticos e alternativa de alto contraste.

O toggle de busca por backing track usa fundo `CubeAccent`, texto sobre destaque e check quando ligado, e aparência neutra sem check quando desligado. Preservar foco, comportamento de ToggleButton e estado localizado para automação. Detalhes de interação e verificação ficam em [Interação no Windows](desenvolvimento/windows-interaction.md#player-e-mixer-de-backing-tracks).

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

Chorus, Phaser, Delay e Reverb usam manipulação direta de contornos, sem sliders, faders ou knobs internos. Os comandos usam botões e ícones WPF UI; os detalhes numéricos aparecem junto ao elemento selecionado. Alvos de 36 DIPs, foco visível e UI Automation compartilham as propriedades do modelo. Desenhos são ilustrativos e não animados; não representam medições. Os controles normais dos pedais permanecem separados. Consulte [mapeamentos e limitações](referencias/editores-graficos-de-efeitos.md).
