const colors = require('tailwindcss/colors')

/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/pages/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/components/**/*.{js,ts,jsx,tsx,mdx}",
    "./src/app/**/*.{js,ts,jsx,tsx,mdx}",
  ],
  theme: {
    extend: {
      backgroundImage: {
        "gradient-radial": "radial-gradient(var(--tw-gradient-stops))",
        "gradient-conic":
          "conic-gradient(from 180deg at 50% 50%, var(--tw-gradient-stops))",
      },
    },
    colors: {
      // must have `transparent` and `current`
      transparent: 'transparent',
      current: 'currentColor',
      // UoN primary colour https://www.nottingham.ac.uk/brand/visual/colour.aspx#Primarycolour
      "uon-blue": {
        100: "#10263B",
        80: "#405162",
        60: "#707D89",
        40: "#9FA8B1",
        20: "#CFD4D8",
        5: "#F3F4F5"
      },
      "uon-bramley": {
        100: "#93D500",
        80: "#A9DD33",
        60: "#BEE666",
        40: "#D4EE99",
        20: "#E9F7CC",
        5: "#FAFDF2"
      },
      "uon-red": {
        100: "#B91C2E",
        80: "#C74958",
        60: "#D57782",
        40: "#E3A4AB",
        20: "#F1D2D5",
        5: "#FCF4F5"
      },
      "uon-sky": {
        100: "#009BC1",
        80: "#33AFCD",
        60: "#66C3DA",
        40: "#99D7E6",
        20: "#CCEBF3",
        5: "#F2FAFC"
      },
      white: colors.white
    },
    backgroundImage: {
      "uon-gradient":
        "linear-gradient(90deg, #10263B 55%, #405162, #009BC1 97%, #33AFCD)",
    }
  },
  plugins: [],
};
