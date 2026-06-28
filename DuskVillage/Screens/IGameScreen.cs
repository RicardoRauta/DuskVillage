using Microsoft.Xna.Framework;

namespace DuskVillage.Screens;

public interface IGameScreen
{
    void OnEnter();

    void OnExit();

    void Update(GameTime gameTime);

    void Draw(GameTime gameTime);
}
