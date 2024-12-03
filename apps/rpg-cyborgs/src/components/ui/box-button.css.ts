import { style } from '@vanilla-extract/css'

export const boxButton = style([
  {
    display: 'inline-block',
    border: '1px solid #ccc',
    borderRadius: '4px',
    boxShadow: '0 0 5px -1px rgba(0, 0, 0, 0.2)',
    cursor: 'pointer',
    verticalAlign: 'middle',
    maxWidth: '100px',
    padding: '5px',
    textAlign: 'center',
    ':hover': {
      boxShadow: '0 0 5px -1px rgba(0, 0, 0, 0.6)',
    },
    ':active': {
      color: 'red',
      boxShadow: '0 0 5px -1px rgba(0, 0, 0, 0.6)',
    },
  },
])

// .boxButton:active {

// }
