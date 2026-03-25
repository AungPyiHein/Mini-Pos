/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './**/*.{razor,html,cshtml}',
    './wwwroot/index.html'
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ['Inter', 'ui-sans-serif', 'system-ui'],
      },
      colors: {
        sidebar: {
          DEFAULT: '#0f1117',
          hover: '#1a1d2e',
        },
        surface: '#1a1d27',
        accent: {
          DEFAULT: '#6366f1',
          hover: '#4f46e5',
          light: '#818cf8',
        },
        content: '#f8fafc',
        card: '#ffffff',
        muted: '#6b7280',
        border: '#e5e7eb',
        'dark-border': '#2d3048',
      },
      boxShadow: {
        card: '0 1px 3px 0 rgb(0 0 0 / 0.07), 0 1px 2px -1px rgb(0 0 0 / 0.07)',
        'card-hover': '0 4px 16px 0 rgb(0 0 0 / 0.10)',
      },
    },
  },
  plugins: [],
}
