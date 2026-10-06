# Design system do cubeControl

[Documentação](README.md) · Revisão: 04/10/2026 · [Decisões históricas](historico-interface.md)

Referência para manutenção da interface WPF/.NET 10 com WPF UI 4.3.0. Esta revisão separa o comportamento atual das decisões antigas, que incluíam regras posteriormente substituídas. Critérios de acessibilidade são metas de verificação, não uma declaração de conformidade já comprovada.

## Referência aprovada e aplicação ao restante do app

A seção **Pedaleira**, incluindo o seletor de presets, a cadeia de efeitos e o editor do módulo selecionado, é a referência visual e de interação aprovada para todo o aplicativo. As adequações futuras de IRs, Dispositivo e ajuda, Backing Tracks e Criador de amps devem seguir esta base. A existência de um estilo particular em outra seção não o torna um padrão global.

O padrão combina superfícies neutras, hierarquia por preenchimento e espaçamento, tipografia Segoe UI, comandos WPF UI e controles musicais com os assets oficiais. Cores de efeitos são acentos de identificação. O conteúdo e a organização funcional de cada seção podem variar; cores, densidade, estados e componentes equivalentes devem permanecer consistentes com a Pedaleira.

Esta revisão documenta a referência existente e os critérios para futuras adequações; não declara que as demais seções já foram adequadas. Os valores abaixo foram conferidos no XAML e nos controles locais, sem uma nova homologação visual ou de acessibilidade nesta revisão.

## Tokens e medidas da referência

Use os recursos existentes, sem copiar os valores hexadecimais para estilos locais. Cores e espessuras dependentes do tema usam `DynamicResource`; estilos compartilhados são referenciados por suas chaves.

| Recurso | Escuro | Claro | Uso |
| --- | --- | --- | --- |
| `CubeCanvas` | `#202020` | `#F3F3F3` | Fundo sólido da aplicação |
| `CubeCard` | `#2B2B2B` | `#FBFBFB` | Cards e seletores em repouso |
| `CubeInset` | `#252525` | `#F5F5F5` | Área de equipamento e preenchimento de hover dos tiles |
| `CubeWell` | `#1C1C1C` | `#EFEFEF` | Áreas rebaixadas |
| `CubeFloatingSurface` | `#333333` | `#FFFFFF` | Superfícies flutuantes |
| `CubeSelected` | `#3B3B3B` | `#E9E9E9` | Seleção |
| `CubeSelectedText` | `#FFFFFF` | `#1B1B1B` | Conteúdo selecionado |
| `CubeBorder` | `#404040` | `#E0E0E0` | Separadores e contornos quando previstos |
| `CubeAccent` | `#BDBDBD` | `#626262` | Acento neutro dos controles musicais |

`CubeText`, `Muted` e `CubeDisabledText` acompanham os recursos de texto primário, secundário e desabilitado do WPF UI. Comandos destacados usam os recursos de destaque do sistema. Não confundir esse destaque com `CubeAccent`. Em alto contraste, usar os recursos do sistema definidos por `ThemeService`; `CubeSurfaceBorderThickness` passa de 0 para 1 DIP. Não acrescentar bordas decorativas permanentes aos cards nos temas claro e escuro.

| Estilo/recurso | Especificação | Aplicação |
| --- | --- | --- |
| `CubeUiFont` | Segoe UI Variable Text → Segoe UI Variable → Segoe UI | Família da interface |
| `TitleText` | 28, SemiBold, entrelinha 36 | Título da página |
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

Todas as medidas são DIPs antes da escala de interface. Para compor novas seções, reutilizar os intervalos da referência: 4–8 entre elementos próximos, 12–16 entre grupos e 24 no interior de superfícies principais. A Pedaleira usa 6 entre título e descrição e 12 após a descrição. Não criar uma escala tipográfica ou densidade própria por página. Ícones de comando usam normalmente `SymbolIcon` de 16 DIPs; ações contextuais, 20; navegação, 24.

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

O switch compartilhado tem altura mínima 32 e face de 56 × 26 com os assets oficiais de ligado/desligado. Presets usam a imagem oficial de footswitch. Esses elementos representam operações diferentes e não devem ser trocados entre si apenas por semelhança visual.

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

A navegação reúne **Pedaleira**, **IRs**, **Dispositivo e ajuda**, **Backing Tracks** e **Criador de amps**. O [histórico](historico-interface.md) descreve fases com apenas três áreas; essa contagem não representa o produto atual.

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

Superfícies claras/escuras usam fundos e espaçamento sem contornos permanentes. O tratamento de alto contraste segue os recursos atuais, incluindo contorno em cards; regras antigas de remover qualquer contorno foram substituídas. Foco precisa permanecer identificável nos controles nativos e musicais.

Detalhes de zoom, rolagem e duração das animações: [Interação no Windows](windows-interaction.md). Os callbacks de renderização não garantem FPS apresentados pelo monitor.

## Componentes

| Componente | Regra atual |
| --- | --- |
| Botões | Reutilizar `CubeButtonStyle` e templates WPF UI; texto indica a ação |
| Seletores de equipamento | Imagem abre galeria; destaque preliminar, aplicar/clique duplo confirma, cancelamento não aplica |
| Knobs da Pedaleira | Valor por tooltip/automação, sem restabelecer campos numéricos permanentes das versões antigas |
| Controles do criador de amps | Faders/knobs, edição numérica por Enter, referência visível e ajuste fino com Shift; respeitar faixas individuais |
| Microfone de IR | Aplica ao concluir o gesto; 0–100 não significa centímetros |
| Gráfico dos EQs | Representa os coeficientes do filtro, sem alegar medição acústica |
| Galeria e associações | Identidades estáveis; gabinete reconhecido pelo conteúdo, não pelo slot |
| Notificações | Snackbar para resultado breve; erro ou orientação importante permanece consultável no editor |
| Confirmações | Manter destino, consequência e backup definidos pelo fluxo; arquivo/slot não pode ficar ambíguo |

O criador possui limites específicos em cinco grupos de coeficientes; o limite atual de ganho interno é 100 e aceita os nove amps originais; veja [Criador de amps](criador-de-amps.md). Não converta os nomes musicais dos grupos em bandas de frequência comprovadas. Os códigos A/B preservam a rastreabilidade. Resposta usa dois painéis com chave discreta, visor seletor e intensidade, reaproveitando PNGs vintage transparentes e cards WPF UI. Os 16 trechos internos ficam ocultos e limitados a 67–133%; os perfis são propostas de escuta, não uma curva de palhetada ou equivalência elétrica comprovada. Em alto contraste os knobs usam desenho vetorial e cores do sistema. A EQ distingue A/B por cor, letra e traço, com reset da banda. Estados locais, prévia RAM e gravação verificada ficam separados. A página usa recursos locais e uma única rolagem por modo de layout.

## Progresso e mensagens

A operação central usa ring quando não existe medida e barra quando há progresso por bytes. O inventário mantém a contagem real no texto com apresentação indeterminada. Não apresente 100% como gravação concluída antes da conferência.

Mensagens devem informar o resultado, a pendência e a próxima ação disponível. Diferencie:

- Edição local, envio confirmado e gravação verificada.
- Prévia iniciada, confirmada e interrompida.
- Preset editado e preset fisicamente audível.
- Referência visual de equipamento e conteúdo identificado.
- Resultado de leitura e retenção após desligamento.

Evite notificações por movimento de knob ou monitoramento automático. Eventos e durações: [Notificações](snackbars.md).

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
- [ ] Seleção, ativação, hover, foco, indisponibilidade e processamento são distinguíveis e coerentes com a referência.
- [ ] A ordem de leitura é clara, ações continuam acessíveis na janela compacta e os grupos se reorganizam com zoom de 50–200% e diferentes DPIs.
- [ ] Imagens preservam proporção e diagramas não sugerem uma medição inexistente.
- [ ] Teclado, nomes acessíveis, foco e textos longos foram verificados em português e inglês; contraste foi medido quando aplicável.
- [ ] Edição local, prévia, envio e gravação mantêm suas consequências explícitas.

Registrar na adequação a seção alterada, os componentes reutilizados, eventuais exceções e a evidência de verificação (tema, tamanho de janela, zoom e idioma). Critérios não verificados ficam marcados como pendentes. Diferenças existentes em outras páginas são candidatas a adequação, não justificativa automática para mudar a referência aprovada.

[Estilos](../windows/Cube.Desktop/App.xaml), [janela](../windows/Cube.Desktop/MainWindow.xaml), [tema](../windows/Cube.Desktop/Appearance/ThemeService.cs), [densidade](../windows/Cube.Desktop/Appearance/UiDensity.cs), [zoom](../windows/Cube.Desktop/Appearance/ZoomService.cs) e [editor de amps](../windows/Cube.Desktop/Views/AmpEditor.xaml) são as fontes da implementação.

Para a referência da Pedaleira, consultar também [layout da cadeia](../windows/Cube.Desktop/Controls/StagePanel.cs), [modelo visual dos módulos](../windows/Cube.Desktop/ViewModels/EditorViewModel.Visual.cs), [knob](../windows/Cube.Desktop/Controls/Knob.cs) e [interações da janela](../windows/Cube.Desktop/MainWindow.xaml.cs). Se código e documento divergirem, conferir a implementação e registrar a decisão antes de propagar a diferença para outras seções.

Registre novas decisões neste guia e mantenha histórico separado. Não altere protocolo, dependências ou semântica de persistência como consequência implícita de uma mudança visual. O antigo relatório `wpf-ui-audit.md` citado no histórico não está incluído nesta árvore; não é uma verificação reproduzida nesta revisão.

## Criadores gráficos de efeitos

Chorus, Phaser, Delay e Reverb usam manipulação direta de contornos, sem sliders, faders ou knobs internos. Os comandos usam botões e ícones WPF UI; os detalhes numéricos aparecem junto ao elemento selecionado. Alvos de 36 DIPs, foco visível e UI Automation compartilham as propriedades do modelo. Desenhos são ilustrativos e não animados; não representam medições. Os controles normais dos pedais permanecem separados. Consulte [mapeamentos e limitações](editores-graficos-de-efeitos.md).
