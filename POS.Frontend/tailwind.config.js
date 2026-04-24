/** @type {import('tailwindcss').Config} */
module.exports = {
  darkMode: 'class',
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
          DEFAULT: 'rgb(var(--color-sidebar) / <alpha-value>)',
          hover: 'rgb(var(--color-sidebar-hover) / <alpha-value>)',
        },
        surface: 'rgb(var(--color-surface) / <alpha-value>)',
        accent: {
          DEFAULT: 'rgb(var(--color-accent) / <alpha-value>)',
          hover: 'rgb(var(--color-accent-hover) / <alpha-value>)',
          light: 'rgb(var(--color-accent-light) / <alpha-value>)',
          glow: 'rgba(var(--color-accent-glow), 0.4)',
        },
        content: 'rgb(var(--color-content) / <alpha-value>)',
        card: 'rgb(var(--color-card) / <alpha-value>)',
        muted: 'rgb(var(--color-muted) / <alpha-value>)',
        border: 'rgb(var(--color-border) / <alpha-value>)',
        'dark-border': 'rgb(var(--color-sidebar-border) / <alpha-value>)',
        'text-main': 'rgb(var(--color-text-main) / <alpha-value>)',
        'text-muted': 'rgb(var(--color-text-muted) / <alpha-value>)',
        'sidebar-text': 'rgb(var(--color-sidebar-text) / <alpha-value>)',
        'sidebar-text-muted': 'rgb(var(--color-sidebar-text-muted) / <alpha-value>)',
      },
      boxShadow: {
        card: '0 4px 6px -1px rgba(0, 0, 0, 0.3), 0 2px 4px -1px rgba(0, 0, 0, 0.06)',
        'card-hover': '0 10px 15px -3px rgba(0, 0, 0, 0.4), 0 4px 6px -2px rgba(0, 0, 0, 0.2)',
        'neon': '0 0 10px rgba(var(--color-accent-glow), 0.3), 0 0 20px rgba(var(--color-accent-glow), 0.2)',
        'neon-strong': '0 0 15px rgba(var(--color-accent-glow), 0.5), 0 0 30px rgba(var(--color-accent-glow), 0.4)',
      },
    },
  },
  plugins: [],
}
