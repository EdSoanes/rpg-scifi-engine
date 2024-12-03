// import globals from 'globals'
// import pluginJs from '@eslint/js'
// import tseslint from 'typescript-eslint'
// import pluginReact from 'eslint-plugin-react'
// //import prettier from 'eslint-plugin-prettier'

// /** @type {import('eslint').Linter.Config[]} */

// const config = [
//   {
//     files: ['src/*.{js,mjs,cjs,ts,jsx,tsx}'],
//     languageOptions: { globals: { ...globals.browser, ...globals.node } },
//     rules: {
//       ...pluginJs.configs.recommended.rules,
//       '@typescript-eslint/no-unused-vars': 'off',
//     },
//   },

//   ...tseslint.configs.recommended,
//   pluginReact.configs.flat.recommended,
// ]

// console.log('eslint', config)
// export default config
import eslint from '@eslint/js'
import tseslint from 'typescript-eslint'
import react from 'eslint-plugin-react'

export default tseslint.config(
  {
    ignores: ['**/*.d.ts', '*.config.{js,mjs}', 'dist/*'],
  },
  eslint.configs.recommended,
  tseslint.configs.recommendedTypeChecked,
  tseslint.configs.stylisticTypeChecked,
  react.configs.flat.recommended,
  react.configs.flat['jsx-runtime'], // Add this if you are using React 17+
  {
    languageOptions: {
      parserOptions: {
        projectService: true,
        tsconfigRootDir: import.meta.dirname,
        ecmaFeatures: {
          jsx: true,
        },
      },
    },
    rules: {
      '@typescript-eslint/no-floating-promises': 'warn',
      '@typescript-eslint/no-misused-promises': 'warn',
    },
  }
)
