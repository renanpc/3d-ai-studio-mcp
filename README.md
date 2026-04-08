# 3D AI Studio MCP Server

`3d-ai-studio-mcp` is a remote MCP server built with ASP.NET Core and the `ModelContextProtocol.AspNetCore` SDK. It exposes 3D AI Studio tools over Streamable HTTP instead of stdio, so MCP clients connect by URL while you run the server as a normal web process.

Available tools:

- `generate_tencent_hunyuan_pro`
- `generate_tencent_hunyuan_rapid`
- `generate_trellis2_model`
- `edit_tencent_hunyuan_texture`
- `remesh_tencent_hunyuan_model`
- `generate_gemini3_pro_image`
- `edit_gemini3_pro_image`
- `generate_gemini31_flash_image`
- `edit_gemini31_flash_image`
- `generate_gemini25_flash_image`
- `edit_gemini25_flash_image`
- `generate_seedream_v5_lite_image`
- `edit_seedream_v5_lite_image`
- `get_generation_status`
- `get_credit_balance`

## Prerequisites

- .NET SDK 10.0+
- A 3D AI Studio API key
- Available credits in your 3D AI Studio account

## Configuration

Set the required API key before starting the server:

```powershell
$env:THREE_D_AI_STUDIO_API_KEY = "your-api-key"
```

Optional overrides:

```powershell
$env:THREE_D_AI_STUDIO_BASE_URL = "https://api.3daistudio.com"
$env:HTTP_PORT = "8080"
$env:ASPNETCORE_URLS = "http://0.0.0.0:6281"
$env:THREE_D_AI_STUDIO_FAILURE_LOG_PATH = "logs/three-d-ai-studio-api-failures.log"
```

`THREE_D_AI_STUDIO_API_KEY` is required for live tool calls. `HTTP_PORT` and `ASPNETCORE_URLS` are optional and let you bind the remote MCP server to a custom interface or port. `THREE_D_AI_STUDIO_FAILURE_LOG_PATH` is optional and controls where failed upstream API calls are appended as text entries.

## Build

```powershell
dotnet restore
dotnet build
```

## Run locally

```powershell
dotnet run --project .
```

By default, the launch profile exposes the MCP server at:

```text
http://localhost:6281
```

## Docker

Build locally:

```bash
docker build -t renanpcf/3d-ai-studio-mcp:local .
```

Use the latest public image from Docker Hub:

```bash
docker pull renanpcf/3d-ai-studio-mcp:latest
docker run --rm -p 8080:8080 \
  -e THREE_D_AI_STUDIO_API_KEY=your-api-key \
  -e THREE_D_AI_STUDIO_BASE_URL=https://api.3daistudio.com \
  -e HTTP_PORT=8080 \
  renanpcf/3d-ai-studio-mcp:latest
```

Run the container:

```bash
docker run --rm -p 8080:8080 \
  -e THREE_D_AI_STUDIO_API_KEY=your-api-key \
  -e THREE_D_AI_STUDIO_BASE_URL=https://api.3daistudio.com \
  -e HTTP_PORT=8080 \
  renanpcf/3d-ai-studio-mcp:local
```

Container environment variables:

- `THREE_D_AI_STUDIO_API_KEY` is required.
- `THREE_D_AI_STUDIO_BASE_URL` is optional and defaults to `https://api.3daistudio.com`.
- `THREE_D_AI_STUDIO_FAILURE_LOG_PATH` is optional.
- `HTTP_PORT` is optional and defaults to `8080`.

## Disclaimer

This project is provided as a developer and integration tool. It is used at your own risk, and you are responsible for reviewing generated outputs, prompts, referenced assets, and downstream usage before relying on them in production workflows.

## VS Code / Copilot configuration

For remote HTTP mode, Copilot does not start the server for you. Run the server separately, then point `.vscode/mcp.json` at the URL:

```json
{
  "servers": {
    "3d-ai-studio-mcp": {
      "type": "http",
      "url": "http://localhost:6281"
    }
  }
}
```

## Tool behavior

`generate_tencent_hunyuan_pro`

- Supports prompt-to-3D, image-to-3D, and multi-view image input.
- Accepts either direct base64 or data-URI image strings, or local file paths that are converted to data URIs automatically.

`generate_tencent_hunyuan_rapid`

- Supports prompt-to-3D and image-to-3D.
- Returns a `task_id` that should be polled with `get_generation_status`.

`generate_trellis2_model`

- Supports image-to-3D generation from either a public `imageUrl`, a local `imageFilePath`, or inline base64 image data.
- Exposes TRELLIS.2 resolution, texture, seed, thumbnail, and decimation controls.

`edit_tencent_hunyuan_texture`

- Applies a new texture to an FBX model from either a text prompt or a reference image.
- Supports local image files for the image-guided mode by converting them to data URIs automatically.

`remesh_tencent_hunyuan_model`

- Calls the Tencent smart-topology endpoint for GLB or OBJ models.
- Supports optional polygon type and density presets.

`generate_gemini3_pro_image`, `generate_gemini31_flash_image`, `generate_gemini25_flash_image`

- Submit text-to-image jobs for the Gemini image models exposed by 3D AI Studio.
- The server validates model-specific aspect ratios, resolutions, and image counts before calling the API.

`edit_gemini3_pro_image`, `edit_gemini31_flash_image`, `edit_gemini25_flash_image`

- Submit image-editing jobs with one or more source images.
- Accepts either inline base64/data URI images or local file paths that are converted automatically.

`generate_seedream_v5_lite_image`, `edit_seedream_v5_lite_image`

- Support Seedream V5 Lite generation and editing, including image size presets, batch size, seed, and safety checker options.
- Editing accepts inline image data or local file paths that are converted automatically.

`get_generation_status`

- Polls `/v1/generation-request/{task_id}/status/`.
- Returns generation progress, failure details, and generated asset URLs when finished.

`get_credit_balance`

- Calls `/account/user/wallet/`.
- Returns the current non-expired API credit balance for the authenticated account.

## Notes

- The MCP endpoint is exposed through ASP.NET Core with `app.MapMcp()`.
- The server starts even if the API key is missing, but tool calls will fail with a clear configuration error until `THREE_D_AI_STUDIO_API_KEY` is set.
- Failed upstream API calls are appended to `logs/three-d-ai-studio-api-failures.log` by default, including request and response JSON details.
- Rapid responses ultimately yield a ZIP archive containing OBJ assets.
- Pro responses yield a GLB file.
- For development, plain `http://localhost` is usually the simplest option for MCP clients.

## References

- [3D AI Studio API Getting Started](https://www.3daistudio.com/Platform/API/Documentation/getting-started)
- [3D AI Studio Tencent Hunyuan 3D API](https://www.3daistudio.com/Platform/API/Documentation/3d-generation/tencent-hunyuan)
- [3D AI Studio TRELLIS.2 API](https://www.3daistudio.com/Platform/API/Documentation/3d-generation/trellis2)
- [3D AI Studio Texturing API](https://www.3daistudio.com/Platform/API/Documentation/texturing/tencent-hunyuan)
- [3D AI Studio Remeshing API](https://www.3daistudio.com/Platform/API/Documentation/remeshing/tencent-hunyuan)
- [3D AI Studio Gemini 3 Pro Image API](https://www.3daistudio.com/Platform/API/Documentation/images/gemini)
- [3D AI Studio Gemini 3.1 Flash Image API](https://www.3daistudio.com/Platform/API/Documentation/images/gemini-31-flash)
- [3D AI Studio Gemini 2.5 Flash Image API](https://www.3daistudio.com/Platform/API/Documentation/images/gemini-25-flash)
- [3D AI Studio Seedream V5 Lite Image API](https://www.3daistudio.com/Platform/API/Documentation/images/seedream)
- [3D AI Studio Credit Balance API](https://www.3daistudio.com/Platform/API/Documentation/credit-balance)
- [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- [Microsoft .NET MCP server quickstart](https://learn.microsoft.com/dotnet/ai/quickstarts/build-mcp-server)
