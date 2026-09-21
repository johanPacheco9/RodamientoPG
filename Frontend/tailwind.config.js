// tailwind.config.js
module.exports = {
  content: [
    "./Frontend/**/*.razor",
    "./Frontend/**/*.cshtml",
    "./Frontend/**/*.js"
  ],
  theme: {
    extend: {
      colors: {
        primary: require('tailwindcss/colors').blue[600],
        success: require('tailwindcss/colors').emerald[600],
        warning: require('tailwindcss/colors').amber[600],
        error: require('tailwindcss/colors').rose[600],
        slate: require('tailwindcss/colors').slate,
      },
    },
  },
  plugins: [require('@tailwindcss/forms'), require('@tailwindcss/typography')],
};
