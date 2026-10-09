# Guias publicados

[Índice geral](../README.md). Fontes em `pt-BR` e `en-US`; catálogo em `catalogo.v1.json`. Publicação automática em `Assets/Documents/guias` pelo projeto desktop.

Cada entrada tem `id` estável, `category` e `versions`. Cada versão informa `language`, `title`, `summary`, `keywords`, `format` (`markdown` ou `pdf`) e `path` relativo à raiz publicada `Assets/Documents`. Guias usam `guias/<idioma>/<id>.md`; manuais usam `manuais/<origem>/<idioma>/<arquivo>.pdf`. Categorias: `comecar`, `timbres`, `criadores`, `praticar`, `ajuda`, `manuais`.

O leitor suporta títulos, parágrafos, listas simples, tabelas, negrito, itálico, código e links. Use títulos únicos quando possível; âncoras conservam acentos, removem pontuação e substituem espaços por hífens. Não inclua HTML, links para relatórios do checkout ou instruções de build nos guias publicados.
