import {
  HTMLChakraProps,
  NumberInputValueChangeDetails,
} from '@chakra-ui/react'
import { StepperInput } from './stepper-input'
import { useState } from 'react'
import { PropValue } from '@/lib/rpg-api/cyborg-types'

export interface PropertyValueProps extends HTMLChakraProps<'div'> {
  propValue?: PropValue
  onPropValueChanged: (value: number) => void
}

export default function PropertyValue(props: PropertyValueProps) {
  const { propValue, onPropValueChanged } = props
  const [hide, setHide] = useState<boolean>(true)

  const propValueChanged = (e: NumberInputValueChangeDetails) => {
    if (propValue && onPropValueChanged) {
      const newBaseValue =
        propValue.baseValue + Number(e.value) - propValue.value
      console.log('orgBaseValue', propValue.originalBaseValue)
      console.log('oldBaseValue', propValue.baseValue)
      console.log('newBaseValue', newBaseValue)
      onPropValueChanged(newBaseValue)
    }
  }

  let color = 'black'
  if (propValue) {
    if (propValue.baseValue > propValue.originalBaseValue) color = 'green'
    else if (propValue.baseValue < propValue.originalBaseValue) color = 'red'
  }

  return (
    <StepperInput
      hideTriggers={hide}
      spinOnPress={false}
      maxW="200px"
      color={color}
      size={'md'}
      value={String(propValue?.value ?? 0)}
      onValueChange={propValueChanged}
      onMouseOver={() => setHide(false)}
      onMouseLeave={() => setHide(true)}
    />
  )
}
