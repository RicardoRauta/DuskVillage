using System.Linq;
using DuskVillage.Core;
using DuskVillage.Saving;
using DuskVillage.UI;
using Microsoft.Xna.Framework;

namespace DuskVillage.Screens;

public sealed class LoadGameScreen : GameScreenBase
{
    private readonly VerticalMenu _menu = new();
    private bool _hasAnySave;

    public LoadGameScreen(GameScreenContext context)
        : base(context)
    {
        RefreshSlots();
    }

    public override void Update(GameTime gameTime)
    {
        if (BackRequested())
        {
            Context.Navigator.Back();
            return;
        }

        _menu.Arrange(CenterX(Context.ViewBounds, 620), 164, 620, 50, 10);
        _menu.Update(Context);
    }

    public override void Draw(GameTime gameTime)
    {
        var draw = BeginUi();
        DrawBackdrop(draw);
        DrawScreenTitle(draw, "load_game.title");

        _menu.Draw(draw);

        if (!_hasAnySave)
        {
            var text = T("load_game.empty_state");
            var size = Context.Font.MeasureString(text) * UiScale * 0.9f;
            draw.Text(text, new Vector2((Context.ViewBounds.Width - size.X) / 2f, Context.ViewBounds.Bottom - 58), draw.Theme.MutedText, 0.9f);
        }

        EndUi();
    }

    private void RefreshSlots()
    {
        _menu.Clear();

        var slots = Context.SaveSlots.GetSlots();
        _hasAnySave = slots.Any(slot => slot.Status != SaveSlotStatus.Empty);

        foreach (var slot in slots)
        {
            var slotCopy = slot;
            var button = new ButtonControl(localization => SlotText(slotCopy), () => LoadSlot(slotCopy))
            {
                IsEnabled = slot.Status == SaveSlotStatus.Valid
            };
            _menu.Add(button);
        }

        _menu.Add(new ButtonControl("common.refresh", RefreshSlots));
        _menu.Add(new ButtonControl("common.back", Context.Navigator.Back));
    }

    private string SlotText(SaveSlotSummary slot)
    {
        return slot.Status switch
        {
            SaveSlotStatus.Empty => T("load_game.slot_empty", slot.SlotNumber),
            SaveSlotStatus.Valid => T("load_game.slot_valid", slot.SlotNumber, slot.PlayerName, slot.CurrentDay, slot.CurrentTime),
            SaveSlotStatus.Incompatible => T("load_game.slot_incompatible", slot.SlotNumber),
            _ => T("load_game.slot_corrupted", slot.SlotNumber)
        };
    }

    private void LoadSlot(SaveSlotSummary slot)
    {
        if (slot.Status != SaveSlotStatus.Valid)
        {
            return;
        }

        var saveGame = Context.SaveSlots.LoadGame(slot.SlotNumber);
        var session = GameSessionSummary.FromSaveSlot(slot, saveGame);
        Context.Navigator.SetRoot(new GameplayPlaceholderScreen(Context, session));
    }
}
