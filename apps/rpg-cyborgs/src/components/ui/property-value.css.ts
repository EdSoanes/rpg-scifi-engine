import { globalStyle, style } from '@vanilla-extract/css'

export const propertyValue = style([
  {
    display: 'inline-block',
    border: '1px solid #ccc',
    borderRadius: '4px',
    boxShadow: '0 0 5px -1px rgba(0, 0, 0, 0.2)',
    cursor: 'pointer',
    verticalAlign: 'middle',
    padding: '5px',
    textAlign: 'center',
  },
])

globalStyle(`${propertyValue} > .showOnHover`, {
  display: 'none',
})

globalStyle(`${propertyValue} > .showOnHover:hover`, {
  display: 'block',
})
