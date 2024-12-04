import { Box, HTMLChakraProps } from '@chakra-ui/react'
import { boxButton } from './box-button.css'
import { useState } from 'react'

export type BoxButtonState = 'on' | 'off'

export interface BoxButtonProps extends HTMLChakraProps<'div'> {
  onButtonClicked?: (buttonState: BoxButtonState) => void
  toggle?: boolean
  state?: BoxButtonState
}

export default function BoxButton(props: BoxButtonProps) {
  const { toggle, onButtonClicked } = props
  const [buttonState, setButtonState] = useState<BoxButtonState>('off')

  const onClicked = () => {
    if (toggle) {
      const newButtonState = buttonState == 'off' ? 'on' : 'off'
      setButtonState(newButtonState)
      if (onButtonClicked) onButtonClicked(newButtonState)
    }
  }

  return (
    <Box className={boxButton} {...props} onClick={onClicked}>
      {props.children}
    </Box>
  )
}
