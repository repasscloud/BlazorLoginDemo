/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './Components/**/*.razor',
    './Pages/**/*.razor',
    './Pages/**/*.razor.cs',
    './Components/**/*.razor.cs',
  ],
  darkMode: 'class',
  theme: {
    extend: {
      colors: {
        // Brand primaries
        sky: {
          brand: '#38bdf8',
        },
        orange: {
          brand: '#f97316',
        },
        // Semantic / status
        success: '#22c55e',
        warning: '#f59e0b',
        danger: '#ef4444',
        info: '#38bdf8',
        purple: {
          vip: '#a855f7',
        },
      },
      fontFamily: {
        sans: ['Inter', 'ui-sans-serif', 'system-ui', 'sans-serif'],
        heading: ['"Plus Jakarta Sans"', 'ui-sans-serif', 'system-ui', 'sans-serif'],
        mono: ['ui-monospace', 'SFMono-Regular', 'Menlo', 'Monaco', 'Consolas', 'monospace'],
      },
      borderRadius: {
        '2xl': '1rem',
        '3xl': '1.5rem',
      },
      backgroundImage: {
        'brand-gradient': 'linear-gradient(135deg, #38bdf8 0%, #f97316 100%)',
        'sky-glow-light': 'radial-gradient(ellipse at top, rgba(56,189,248,0.16) 0%, transparent 60%)',
        'sky-glow-dark': 'radial-gradient(ellipse at top, rgba(56,189,248,0.12) 0%, transparent 60%)',
      },
    },
  },
  plugins: [
    require('@tailwindcss/forms'),
  ],
}
