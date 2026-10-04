# Design system do cubeControl

[Documentação](README.md) · Revisão: 01/10/2026 · [Decisões históricas](historico-interface.md)

Referência para manutenção da interface WPF/.NET 10 com WPF UI 4.3.0. Esta revisão separa o comportamento atual das decisões antigas, que incluíam regras posteriormente substituídas. Critérios de acessibilidade são metas de verificação, não uma declaração de conformidade já comprovada.

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
- A cadeia permanece uma faixa horizontal. A roda sobre ela navega entre módulos; não restaure a antiga proposta de duas linhas como se fosse o comportamento atual.

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

[Estilos](../windows/Cube.Desktop/App.xaml), [janela](../windows/Cube.Desktop/MainWindow.xaml), [tema](../windows/Cube.Desktop/Appearance/ThemeService.cs), [densidade](../windows/Cube.Desktop/Appearance/UiDensity.cs), [zoom](../windows/Cube.Desktop/Appearance/ZoomService.cs) e [editor de amps](../windows/Cube.Desktop/Views/AmpEditor.xaml) são as fontes da implementação.

Registre novas decisões neste guia e mantenha histórico separado. Não altere protocolo, dependências ou semântica de persistência como consequência implícita de uma mudança visual. O antigo relatório `wpf-ui-audit.md` citado no histórico não está incluído nesta árvore; não é uma verificação reproduzida nesta revisão.

## Criadores gráficos de efeitos

Chorus, Phaser, Delay e Reverb usam manipulação direta de contornos, sem sliders, faders ou knobs internos. Os comandos usam botões e ícones WPF UI; os detalhes numéricos aparecem junto ao elemento selecionado. Alvos de 36 DIPs, foco visível e UI Automation compartilham as propriedades do modelo. Desenhos são ilustrativos e não animados; não representam medições. Os controles normais dos pedais permanecem separados. Consulte [mapeamentos e limitações](editores-graficos-de-efeitos.md).
