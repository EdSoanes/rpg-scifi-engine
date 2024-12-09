import { HStack, IconButton, NumberInput } from '@chakra-ui/react'
import * as React from 'react'
import { LuChevronDown, LuChevronUp } from 'react-icons/lu'

export interface StepperInputProps extends NumberInput.RootProps {
  label?: React.ReactNode
  hideTriggers: boolean
}

export const StepperInput = React.forwardRef<HTMLDivElement, StepperInputProps>(
  function StepperInput(props, ref) {
    const { label, hideTriggers, ...rest } = props

    return (
      <NumberInput.Root {...rest} unstyled ref={ref}>
        {label && <NumberInput.Label>{label}</NumberInput.Label>}
        <HStack gap={0}>
          <DecrementTrigger opacity={hideTriggers ? '0' : '100'} />
          <NumberInput.ValueText
            textAlign="center"
            textStyle="2xl"
            minW="3ch"
          />
          <IncrementTrigger opacity={hideTriggers ? '0' : '100'} />
        </HStack>
      </NumberInput.Root>
    )
  }
)

const DecrementTrigger = React.forwardRef<
  HTMLButtonElement,
  NumberInput.DecrementTriggerProps
>(function DecrementTrigger(props, ref) {
  return (
    <NumberInput.DecrementTrigger {...props} asChild ref={ref}>
      <IconButton variant="outline" size="xs">
        <LuChevronDown />
      </IconButton>
    </NumberInput.DecrementTrigger>
  )
})

const IncrementTrigger = React.forwardRef<
  HTMLButtonElement,
  NumberInput.IncrementTriggerProps
>(function IncrementTrigger(props, ref) {
  return (
    <NumberInput.IncrementTrigger {...props} asChild ref={ref}>
      <IconButton variant="outline" size="xs">
        <LuChevronUp />
      </IconButton>
    </NumberInput.IncrementTrigger>
  )
})
