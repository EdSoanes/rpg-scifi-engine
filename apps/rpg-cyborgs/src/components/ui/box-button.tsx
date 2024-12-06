import { Box, HTMLChakraProps } from '@chakra-ui/react'
import { boxButton } from './box-button.css'

export type BoxButtonState = 'on' | 'off'

export interface BoxButtonProps extends HTMLChakraProps<'div'> {
  state?: BoxButtonState
}

export default function BoxButton(props: BoxButtonProps) {
  return (
    <Box className={boxButton} {...props}>
      {props.children}
    </Box>
  )
}
