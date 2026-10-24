# cubeControl: design system

## Identidade e alcance

Interface Windows/Fluent com personalidade de equipamento musical. Preservar imagens de equipamentos, knobs e gráficos quando ajudam a reconhecer ou ajustar o som. Aplicar estas convenções à Pedaleira, IRs, Criador de amps e Dispositivo e ajuda. Backing Tracks fica fora desta consolidação.

## Fundamentos

Os recursos de `Cube.Desktop/Appearance/Resources` são a fonte de verdade; `ThemeService` resolve as cores por tema e alto contraste.

| Papel | Convenção |
| --- | --- |
| Espaçamento | 4, 8, 12, 16, 24 e 32 DIP (`CubeSpaceXS` até `CubeSpaceXXL`) |
| Página e cartão padrão | 24 DIP de margem ou padding; grupos compactos podem usar 16 |
| Controles | 40 DIP padrão; 32 DIP na variante compacta existente |
| Cantos | 4 DIP em cartões e controles; 8 em popups |
| Tipografia | Segoe UI Variable, com fallback Segoe UI |
| Corpo e legenda | 14/20 e 12/16 DIP, respectivamente |
| Títulos | Página 28/36; seção 20/28; peso SemiBold |

Usar `CubePageTitle` e `CubePageDescription` nos cabeçalhos. A descrição tem espaçamento inferior de 16 DIP. Usar `SectionTitle`, `BodyText`, `BodyStrongText` e `HelpText` para os demais papéis, evitando reconstruir estilos com propriedades locais.

Superfícies e texto usam `CubeCanvas`, `CubeCard`, `CubeInset`, `CubeWell`, `CubeText` e `Muted`. Seleção usa `CubeSelected` e `CubeSelectedText`; foco usa `CubeFocus`. Não usar cor como único indicador de estado.

As cores `CubeAmpAccent`, `CubeAmpBandA`, `CubeAmpBandB`, `CubeCapacitorAccent` e `CubeCapacitorDisplay` pertencem aos instrumentos. Possuem variantes claras/escuras e substituição por cores do sistema em alto contraste. Não representam sucesso, erro ou alteração persistida.

## Componentes e interação

- Cartão de conteúdo: `Card`. Acesso a outra área: `QuickAction`. Conteúdo secundário expansível: `SettingsExpander`.
- Navegação interna: `CubeWorkspaceTabs` com `CubeWorkspaceTab`. Quando cabeçalho e conteúdo ocupam regiões distintas, usar `CubeWorkspaceNavigation` com o mesmo estilo de item. Manter semântica nativa de abas, seleção persistente e foco de teclado visível.
- Ações: um botão primário por grupo de tarefa. Salvar/gravar recebe destaque no contexto de destino; encerrar prévia é secundário.
- Arquivos: Importar/Exportar. Audição temporária: Prévia/Encerrar prévia. Persistência: Salvar presets, Gravar IR ou Salvar no pedal, indicando o objeto ou destino próximo da ação.
- Prévia de amp ativa é informação, pois não significa falha. Manter aviso textual de que nenhum slot foi gravado e uma única ação de encerramento no rodapé.
- Alterações pendentes, processamento e resultados precisam de texto próximo da ação. Mensagens dinâmicas relevantes devem usar anúncio acessível discreto (`LiveSetting=Polite`).
- Instruções essenciais ficam visíveis. Tooltips e expansores guardam detalhes complementares.

## Composição por seção

- Pedaleira: título alinhado ao nome da navegação, presets e controles musicais como foco; preservar indicação de alterações pendentes e separação entre arquivos e gravação.
- IRs: biblioteca, análise/prévia e destino de gravação preservam suas funções. A ação de encerrar prévia não compete visualmente com a gravação.
- Criador de amps: mesma família de abas internas dos IRs; cores de instrumentos resolvidas pelo tema; rodapé reúne destino, aviso de prévia e ações.
- Dispositivo e ajuda: diferenciar o manual do aplicativo dos guias/manuais do pedal; separar visualmente Recuperação e Diagnóstico; usar caixa de sentença nos títulos.

## Validação manual pendente

Esta alteração não executa build nem testes. Na validação manual, conferir temas claro/escuro e alto contraste; escalas 100%, 125%, 150% e 200%; janela mínima e ampliada; português/inglês; abas por teclado e foco visível. Conferir estados desconectado, vazio, carregado, prévia, alterações pendentes, processamento, sucesso e falha. Verificar que gravação, confirmação, backup e encerramento de prévia mantêm o comportamento anterior. Conferir contraste mínimo de 4,5:1 para texto comum e 3:1 para texto grande e indicadores essenciais. Conferir também ausência de regressões de recursos compartilhados em Backing Tracks.
