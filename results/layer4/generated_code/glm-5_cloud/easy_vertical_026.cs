using System;
using System.Collections.Generic;
using System.Linq;

public record Ingredient(string Name, int Quantity, string Unit);

public record Recipe(string Name, List<Ingredient> Ingredients, int Servings);

public class RecipeScaler
{
    public static Recipe Scale(Recipe recipe, int targetServings)
    {
        if (recipe.Servings <= 0)
        {
            throw new ArgumentException("Original recipe servings must be greater than zero.", nameof(recipe));
        }

        if (targetServings <= 0)
        {
            throw new ArgumentException("Target servings must be greater than zero.", nameof(targetServings));
        }

        var scaledIngredients = recipe.Ingredients.Select(ingredient =>
        {
            double scaledQuantity = (double)ingredient.Quantity * targetServings / recipe.Servings;
            int finalQuantity = (int)Math.Round(scaledQuantity);
            return ingredient with { Quantity = finalQuantity };
        }).ToList();

        return recipe with { Ingredients = scaledIngredients, Servings = targetServings };
    }
}