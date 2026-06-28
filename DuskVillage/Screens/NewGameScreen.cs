using System.Collections.Generic;
using DuskVillage.Core;
using DuskVillage.UI;
using Microsoft.Xna.Framework;

namespace DuskVillage.Screens;

public sealed class NewGameScreen : GameScreenBase
{
    private readonly NewGameOptions _options = new();
    private readonly VerticalMenu _menu = new();

    public NewGameScreen(GameScreenContext context)
        : base(context)
    {
        BuildControls();
    }

    public override void Update(GameTime gameTime)
    {
        if (BackRequested())
        {
            Context.Navigator.Back();
            return;
        }

        _menu.Arrange(CenterX(Context.ViewBounds, 560), 174, 560, 52, 12);
        _menu.Update(Context);
    }

    public override void Draw(GameTime gameTime)
    {
        var draw = BeginUi();
        DrawBackdrop(draw);
        DrawScreenTitle(draw, "new_game.title");

        _menu.Draw(draw);

        var hint = T("new_game.hint");
        draw.Text(hint, new Vector2(CenterX(Context.ViewBounds, 760), Context.ViewBounds.Bottom - 70), draw.Theme.MutedText, 0.82f);

        EndUi();
    }

    private void BuildControls()
    {
        _menu.Add(new TextInputControl(
            "new_game.player_name",
            () => _options.PlayerName,
            value => _options.PlayerName = value,
            18));

        _menu.Add(new SelectorControl(
            "new_game.age",
            new List<SelectorOption>
            {
                new("young_adult", "age.young_adult"),
                new("adult", "age.adult"),
                new("older_adult", "age.older_adult")
            },
            () => _options.AgeCategoryId,
            value => _options.AgeCategoryId = value));

        _menu.Add(new SelectorControl(
            "new_game.origin",
            new List<SelectorOption>
            {
                new("newcomer", "origin.newcomer"),
                new("local_villager", "origin.local_villager"),
                new("former_laborer", "origin.former_laborer"),
                new("poor_wanderer", "origin.poor_wanderer")
            },
            () => _options.OriginId,
            value => _options.OriginId = value));

        _menu.Add(new ButtonControl("new_game.begin", BeginNewGame));
        _menu.Add(new ButtonControl("common.back", Context.Navigator.Back));
    }

    private void BeginNewGame()
    {
        var session = GameSessionSummary.FromNewGame(_options);
        Context.Navigator.SetRoot(new GameplayPlaceholderScreen(Context, session));
    }
}
