using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using static ImGuiNET.ImGui;

namespace SCMonoGameUtilities.DearImGui.Demos.GuiElements.MiniApps;

// Demo of a window thhat includes a rendered 3D image.
class ModelViewerWindow(
    ImGuiRenderer imGuiRenderer,
    GraphicsDevice graphicsDevice,
    ContentManager contentManager,
    string modelAssetName,
    bool isVisible = false)
{
    public bool IsVisible = isVisible;

    private Model model;
    private Matrix modelWorldTransform = Matrix.CreateRotationX(-1.5f);
    private RenderTarget2D modelRenderTarget;
    private nint modelTextureId;

    public void LoadContent()
    {
        model = contentManager.Load<Model>(modelAssetName);
        foreach (ModelMesh mesh in model.Meshes)
        {
            foreach (BasicEffect effect in mesh.Effects)
            {
                effect.TextureEnabled = false;
                effect.EnableDefaultLighting();
                effect.View = Matrix.CreateLookAt(new(0, 0, 4), Vector3.Zero, Vector3.UnitY);
                effect.Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), graphicsDevice.Viewport.AspectRatio, 0.1f, 100.0f);
            }
        }

        modelRenderTarget = new RenderTarget2D(
            graphicsDevice,
            graphicsDevice.PresentationParameters.BackBufferWidth,
            graphicsDevice.PresentationParameters.BackBufferHeight,
            false,
            graphicsDevice.PresentationParameters.BackBufferFormat,
            DepthFormat.Depth24);

        modelTextureId = imGuiRenderer.RegisterTexture(modelRenderTarget);
    }

    public void UnloadContent()
    {
        contentManager.UnloadAsset(modelAssetName);
        imGuiRenderer.UnregisterTexture(modelTextureId);
    }

    public void Update()
    {
        if (!IsVisible) return;

        if (Begin("Example: Model Viewer", ref IsVisible))
        {
            Image(modelTextureId, new(GetContentRegionAvail().X, GetContentRegionAvail().X / graphicsDevice.Viewport.AspectRatio));

            PushItemFlag(ImGuiItemFlags.ButtonRepeat, true);

            Indent();
            if (ArrowButton("up", ImGuiDir.Up))
            {
                modelWorldTransform *= Matrix.CreateRotationX(-0.1f);
            }
            Unindent();
            if (ArrowButton("left", ImGuiDir.Left))
            {
                modelWorldTransform *= Matrix.CreateRotationY(-0.1f);
            }
            SameLine();
            Text(" ");
            SameLine();
            if (ArrowButton("right", ImGuiDir.Right))
            {
                modelWorldTransform *= Matrix.CreateRotationY(0.1f);
            }

            Indent();
            if (ArrowButton("down", ImGuiDir.Down))
            {
                modelWorldTransform *= Matrix.CreateRotationX(0.1f);
            }
            Unindent();
            PopItemFlag();
        }

        End();
    }

    public void DrawModel()
    {
        if (!IsVisible) return;

        var priorRenderTargets = graphicsDevice.GetRenderTargets();

        graphicsDevice.SetRenderTarget(modelRenderTarget);
        graphicsDevice.Clear(Color.Transparent);

        foreach (ModelMesh mesh in model.Meshes)
        {
            foreach (BasicEffect effect in mesh.Effects)
            {
                effect.World = modelWorldTransform;
            }

            mesh.Draw();
        }

        graphicsDevice.SetRenderTargets(priorRenderTargets);
    }
}
