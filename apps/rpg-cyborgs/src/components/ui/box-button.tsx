import { Box, HTMLChakraProps } from '@chakra-ui/react'
import { boxButton } from './box-button.css'
import { useState } from 'react'

export interface BoxButtonProps extends HTMLChakraProps<'div'> {
  onButtonClicked?: (toggleState: string) => void
  toggle?: boolean
}

export default function BoxButton(props: BoxButtonProps) {
  const { toggle, onButtonClicked } = props
  const [isOn, setIsOn] = useState<boolean>(false)

  const onClicked = () => {
    if (toggle) {
      const newIsOn = !isOn
      setIsOn(newIsOn)
      if (onButtonClicked) onButtonClicked(newIsOn ? 'on' : 'off')
    }
  }

  return (
    <Box className={boxButton} {...props} onClick={onClicked}>
      {props.children}
    </Box>
  )
}
