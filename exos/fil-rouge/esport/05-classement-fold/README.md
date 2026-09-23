# Exercice 05 — Classement de saison

## Concepts théoriques

- [Fold — l'agrégation universelle](../../../../supports/source/04-Reduce.md)
- [GroupBy — agrégation par clé](../../../../supports/source/04-Reduce.md#groupby)

## Contexte

Établir le classement officiel des 5 joueurs de Team Helvetia pour la saison.
Le coaching staff veut savoir : qui est le plus régulier ? qui progresse le plus vite ?

`.Fold()` est l'outil fondamental qui permet de répondre à toutes ces questions
en une seule abstraction.

---

## 5.1 — Extraire des indicateurs numériques

Ajouter des capacités à notre application qui permettent d'obtenir des valeurs spécifiques à partir à partir des stats de nos joueurs.

Le programme doit proposer dans son help:

`--extract min|max|avg|mme`

... et naturellement répondre à ces demandes.  
<details>
<summary>Exemples d'utilisation</summary>

```
EsportApp --game cs2 --player Raphaël --e
                                           
CS2 : 25 matchs, 0 écarté(s), 13 retenu(s) 
  2024-01-05  Raphaël   kills = 24.00      
  2024-01-10  Raphaël   kills = 19.00      
  2024-01-14  Raphaël   kills = 22.00      
  2024-01-19  Raphaël   kills = 17.00      
  2024-01-24  Raphaël   kills = 25.00      
  2024-01-29  Raphaël   kills = 16.00      
  2024-02-03  Raphaël   kills = 23.00      
  2024-02-07  Raphaël   kills = 18.00      
  2024-02-12  Raphaël   kills = 22.00      
  2024-02-17  Raphaël   kills = 14.00      
  2024-02-22  Raphaël   kills = 24.00      
  2024-02-27  Raphaël   kills = 20.00      
  2024-03-04  Raphaël   kills = 21.00      
  Min (kills) : 14.00                      
```
                                           
```
EsportApp --game cs2 --player Kiara --ext
                                           
CS2 : 25 matchs, 0 écarté(s), 12 retenu(s) 
  2024-01-06  Kiara     kills = 18.00      
  2024-01-11  Kiara     kills = 21.00      
  2024-01-15  Kiara     kills = 15.00      
  2024-01-20  Kiara     kills = 23.00      
  2024-01-25  Kiara     kills = 22.00      
  2024-01-30  Kiara     kills = 24.00      
  2024-02-04  Kiara     kills = 19.00      
  2024-02-08  Kiara     kills = 26.00      
  2024-02-13  Kiara     kills = 16.00      
  2024-02-18  Kiara     kills = 22.00      
  2024-02-23  Kiara     kills = 23.00      
  2024-02-28  Kiara     kills = 25.00      
  Max (kills) : 26.00                      
```
                                           
```
EsportApp --game lol --extract avg --stat
                                           
LoL : 25 matchs, 0 écarté(s), 25 retenu(s) 
  2024-01-07  Noé       assists = 18.00    
  2024-01-12  Noé       assists = 15.00    
  2024-01-16  Noé       assists = 20.00    
  2024-01-21  Noé       assists = 22.00    
  2024-01-26  Noé       assists = 17.00    
  2024-01-31  Noé       assists = 19.00    
  2024-02-05  Noé       assists = 14.00    
  2024-02-09  Noé       assists = 21.00    
  2024-02-13  Noé       assists = 16.00    
  2024-02-16  Noé       assists = 23.00    
  2024-02-20  Noé       assists = 18.00    
  2024-02-24  Noé       assists = 20.00    
  2024-02-28  Noé       assists = 15.00    
  2024-03-03  Noé       assists = 24.00    
  2024-03-07  Noé       assists = 17.00    
  2024-03-11  Noé       assists = 22.00    
  2024-03-15  Noé       assists = 19.00    
  2024-03-19  Noé       assists = 25.00    
  2024-03-23  Noé       assists = 16.00    
  2024-03-27  Noé       assists = 21.00    
  2024-04-01  Noé       assists = 18.00    
  2024-04-06  Noé       assists = 20.00    
  2024-04-11  Noé       assists = 23.00    
  2024-04-16  Noé       assists = 15.00    
  2024-04-21  Noé       assists = 22.00    
  Moyenne (assists) : 19.20                
```
                                           
</details>

Ces indicateurs sont bien utiles, mais le staff de coaching de Team Helvetia est décidément très pointu et exigeant.  
Il demande un indicateur particulier: la MME, qui signifie "Moyenne Mobile Exponentielle".  

<details>
<summary>Pour tout savoir sur la MME</summary>
c'est [ici](https://fr.wikipedia.org/wiki/Moyenne_mobile#Moyenne_mobile_exponentielle).  
</details>

Ici, il vous suffit de savoir deux choses:
- La MME donne plus de poids dans une moyenne aux dernières valeurs de la série et c'est pour ça qu'elle intéresse Team Helvetia: elle permet de dire qui est le joueur en forme du moment.
- Team Helvetia calcule cette moyenne comme ça: la MME de la série est égale à la moyenne arithmétique de
  - La dernière valeur de la série
  - La MME de toutes les valeurs précédentes.

**Avant de coder :** comment appliquer `Aggregate` dans ce cas de figure ?

<details>
<summary>Indice</summary>

| Indice | Valeur |  MME  | `Sum` | `Average` |
| :----: | :----: | :---: | :---: | :-------: |
|   1    |   10   |  10   |  10   |    10     |
|   2    |   12   |  11   |  22   |    11     |
|   3    |   23   |  17   |  45   |    15     |
|   4    |   55   |  36   |  100  |    25     |
|   5    |   20   |  28   |  120  |    20     |
|   6    |   30   |  29   |  150  |    25     |
|   7    |   53   |  41   |  203  |    29     |
|   8    |   37   |  39   |  240  |    30     |

</details>

Implémenter le calcul de la MME avec `Aggregate` dans la librairie `DataSerie`

```csharp
public double MME(Func<T, double> value)
{
    // ... à vous de jouer
}
```

L'utiliser dans ESportApp avec `--extract mme`

<details>
<summary>Exemples d'utilisation</summary>

```
EsportApp --game cs2 --player Kiara --extract mme --stat kills 
                                                         
CS2 : 25 matchs, 0 écarté(s), 12 retenu(s)               
  2024-01-06  Kiara     kills = 18.00                    
  2024-01-11  Kiara     kills = 21.00                    
  2024-01-15  Kiara     kills = 15.00                    
  2024-01-20  Kiara     kills = 23.00                    
  2024-01-25  Kiara     kills = 22.00                    
  2024-01-30  Kiara     kills = 24.00                    
  2024-02-04  Kiara     kills = 19.00                    
  2024-02-08  Kiara     kills = 26.00                    
  2024-02-13  Kiara     kills = 16.00                    
  2024-02-18  Kiara     kills = 22.00                    
  2024-02-23  Kiara     kills = 23.00                    
  2024-02-28  Kiara     kills = 25.00                    
  MME (kills) - forme du moment : 23.46                  
```

```
EsportApp --game cs2 --player Raphaël --mme

CS2 : 25 matchs, 0 écarté(s), 13 retenu(s)
  2024-01-05  Raphaël   kda = 3.00
  2024-01-10  Raphaël   kda = 2.09
  2024-01-14  Raphaël   kda = 3.00
  2024-01-19  Raphaël   kda = 1.90
  2024-01-24  Raphaël   kda = 3.62
  2024-01-29  Raphaël   kda = 1.58
  2024-02-03  Raphaël   kda = 3.50
  2024-02-07  Raphaël   kda = 2.00
  2024-02-12  Raphaël   kda = 3.12
  2024-02-17  Raphaël   kda = 1.38
  2024-02-22  Raphaël   kda = 3.50
  2024-02-27  Raphaël   kda = 2.50
  2024-03-04  Raphaël   kda = 2.67
  MME (kda) - forme du moment : 2.66
```
</details>

## 5.2 - Défaillances et Surpuissances

ESportApp doit permettre de répondre aux questions que se pose le staff de coaching:
- "Ma joueuse a-t-elle eu des moments d'invincibilité ?"
- "Mon joueur a-t-il eu des défaillances graves ?"

Pour cela, ils veulent interroger les statistiques avec:  
`ESportApp --player Noé --hasCrushed value`  
`ESportApp --player Dylan --hasBeenCrushed value`  
`ESportApp --player Kiara --hasBeenGod value`  

L'app répond par `Yes` ou `No`  

Pour implémenter cette fonctionnalité, basez-vous sur le KDA:

- `hasCrushed` = yes si le joueur a eu au moins un KDA supérieur à `value` dans les parties sélectionnées
- `hasBeenCrushed` = yes si le joueur a eu au moins un KDA inférieur à `value` dans les parties sélectionnées
- `hasBeenGod` = yes si le joueur a eu tous ses KDA supérieurs à `value` dans les parties sélectionnées

N'oubliez pas la cheatsheet (`Any`, `All`)

Attention: les options `--has*` ne peuvent fonctionner qu'avec une sélection de joueur `--player`

## 5.3 — Progression mensuelle

Le staff veut voir la progression - ou non - dans le temps d'un joueur.

Pour cette fonctionnalité, les sélecteurs de joueur `--player` et de jeu `--game` sont requis.

L'objectif est d'avoir le KDA moyen du joueur par mois.

<details>
<summary>Exemples d'utilisation</summary>

```
EsportApp --player Noé --game lol --progress  

2024-01     4.20                                
2024-02     4.40                                
2024-03     5.35                                
2024-04     4.25                                
```
</details>

Construire le pipeline a quatre temps :

1. **Obtenir la liste pertinente de `(timestamp, kda)`** — ne garder que les matchs du joueur et du jeu demandés, puis réduire chaque match à ce qui compte ici : quand il a été joué, et le KDA qu'il a produit. Tout le reste (agent, map, champion, MVPs...) ne sert plus à rien pour la suite.
2. **Transformer en `(date, kda)`** — un timestamp est trop précis pour regrouper : deux matchs du même mois ont deux timestamps différents et formeraient deux groupes. Ramener le timestamp au mois (`yyyy-MM`) : c'est lui, la clé de regroupement.
3. **Grouper par date** — `GroupBy` sur cette clé. Attention : `GroupBy` ne réduit rien, il réorganise. À ce stade, on n'a pas encore de moyennes mais une liste de groupes, chacun contenant tous les KDA d'un même mois.
4. **Réduire au sein de chaque groupe** — chaque groupe est une série de nombres, donc la moyenne de ses KDA donne la valeur du mois. C'est un `Fold` par clé : le motif est `GroupBy(clé).Select(g => g.Aggregate(...))`.

Reste à afficher les mois dans l'ordre — un groupe ne sort pas forcément trié.

→ [GroupBy — agréger par clé](../../../../supports/source/04-Reduce.md#groupby)

