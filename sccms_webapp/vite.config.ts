import { defineConfig } from "vite"
import react from "@vitejs/plugin-react"

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [react()],
    assetsInclude: ["**/*.svg", "**/*.woff", "**/*.woff2", "**/*.eot", "**/*.ttf", "**/*.otf"],
})
