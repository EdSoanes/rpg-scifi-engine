'use client'

import {
  Box,
  Heading,
  HTMLChakraProps,
  Stack,
  ConditionalValue,
} from '@chakra-ui/react'
import { StepperInput } from './stepper-input'
import { useState } from 'react'
import DescribePropertyDrawer from '@components/describe/DescribePropertyDrawer'

export interface PropertyValueProps extends HTMLChakraProps<'div'> {
  name: string
  entityId: string
  prop: string
  value: number
  baseValue?: number
  originalBaseValue?: number

  direction?: ConditionalValue<
    'row' | 'column' | 'row-reverse' | 'column-reverse'
  >
  onPropValueChanged: (value: number) => void
}

export default function PropertyValue(props: PropertyValueProps) {
  const {
    name,
    entityId,
    prop,
    value,
    baseValue,
    originalBaseValue,
    direction,
    onPropValueChanged,
  } = props
  const [hide, setHide] = useState<boolean>(true)

  const onChange = (value: string) => {
    onPropValueChanged(Number(value))
  }

  let color = 'black'
  const bv = baseValue ?? 0
  const obv = originalBaseValue ?? 0
  if (bv > obv) color = 'cyan.600'
  else if (bv < obv) color = 'red'

  return (
    <Box
      p={1}
      m={1}
      borderWidth="1px"
      borderColor="border.disabled"
      color="fg.disabled"
    >
      <Stack direction={direction ?? 'row'}>
        <DescribePropertyDrawer entityId={entityId} prop={prop}>
          <Heading _hover={{ cursor: 'pointer' }} as={'h3'} size={'xs'}>
            {name}
          </Heading>
        </DescribePropertyDrawer>
        <StepperInput
          hideTriggers={hide}
          spinOnPress={false}
          maxW="200px"
          color={color}
          size={'md'}
          value={String(value)}
          onValueChange={(e) => onChange(e.value)}
          onMouseOver={() => setHide(false)}
          onMouseLeave={() => setHide(true)}
        />
      </Stack>
    </Box>
  )
}
