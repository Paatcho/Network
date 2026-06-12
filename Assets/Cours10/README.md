RENDU SHADERGRAPH CRISTAL

Modifications apportées :
- Ajout de réfraction. Les éléments derrière la glace sont déformés selon la forme de la texture.
- Ajout d'effet "lentille". Les éléments derrière la glace sont grossis (de manière exagérée) pour simuler un effet de lentille et/ou de reflet. La screen color et la screen position sont utilisées, donc l'effet peut ne pas être totalement réaliste, et faire apparaître les objets à des endroits où ils ne devraient pas, mais c'est une approximation plutôt efficace.
- Ajout d'un effet de fonte de la glace. Une normal texture de goutelettes est ajoutée, déplacée au fil du temps et déformé pour simuler une glace mouillée qui fond. (les goutelettes participent à la réfraction)