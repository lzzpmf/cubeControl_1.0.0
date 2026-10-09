# Inventário e classificação documental

Revisão: 08/10/2026. [Índice geral](README.md).

| Local | Público e finalidade | Publicado no aplicativo |
| --- | --- | --- |
| `usuario/pt-BR` e `usuario/en-US` | Guias de operação, solução de problemas e privacidade | Sim, conforme catálogo |
| `desenvolvimento` | Arquitetura, integração, interação WPF, notificações, QA e manutenção | Não |
| `referencias` | Contratos de efeitos, formatos, catálogos e fontes de interpretação | Não; conteúdo operacional extraído para guias |
| `historico` | Investigações anteriores, decisões substituídas e guias mistos preservados | Não |
| `DESIGN_SYSTEM.md` | Referência normativa da interface, mantida no caminho original | Não |
| `Assets/Documents/manuais` | Manuais do pedal separados por origem/idioma, com fontes Markdown preservadas | PDFs cadastrados |
| `engenharia-reversa` | Relatórios, fontes Android e artefatos originais de pesquisa | Não |
| READMEs na raiz, Windows, componentes, localização e recursos | Entradas do acervo e manutenção local | Somente o LEIA-ME do pacote existente |
| `scripts/qa` | Operação das ferramentas de QA e verificações | Não |
| Licenças, avisos e documentos dentro do APK/vendor | Documentos de terceiros, preservados junto às fontes | Conforme empacotamento existente de avisos |
| `tmp` e `output` | Renderizações, prompts e artefatos de trabalho | Não |

## Fontes dos guias

Primeiros passos, presets, Pedaleira, IRs, problemas e recuperação foram extraídos do guia misto preservado no histórico e confrontados com os comandos e estados atuais da interface. Criador de amps usa a referência técnica preservada. Os guias de chorus/phaser/delay/reverb extraem os procedimentos operacionais dos relatórios de engenharia reversa, sem publicar endereços, dumps ou resultados de ensaios como instruções. Backing Tracks e simulação têm guias próprios separados do histórico de implementação. A política de privacidade foi movida para a fonte pt-BR e traduzida, com indicação do acesso à rede solicitado pelo recurso YouTube já existente.

## Preservação e verificações

Os três PDFs conservaram seus SHA-256 durante a movimentação. Fontes Markdown conservam o conteúdo e a autoria, com links rebased para a nova organização. O registro de caminhos e hashes desta organização está em [documentos-movidos.v1.json](documentos-movidos.v1.json). O manifesto anterior de assets continua como evidência da etapa anterior, não como catálogo de publicação.

Execute `python scripts/qa/validate_document_links.py` na raiz para conferir links e âncoras do acervo mantido. Execute `scripts/Validate-Documentation.ps1` para validar catálogo, traduções, PDFs e links publicados. O build oficial executa essa segunda validação também contra os arquivos finais do pacote.
