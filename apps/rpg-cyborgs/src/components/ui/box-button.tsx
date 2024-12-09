import { Box, HTMLChakraProps } from '@chakra-ui/react'

export type BoxButtonState = 'on' | 'off'

export interface BoxButtonProps extends HTMLChakraProps<'div'> {
  state?: BoxButtonState
}

export default function BoxButton(props: BoxButtonProps) {
  return <Box {...props}>{props.children}</Box>
}
