import {
  Heading,
  HTMLChakraProps,
  NumberInputValueChangeDetails,
  VStack,
} from '@chakra-ui/react'
import { StepperInput } from './stepper-input'
import { useState } from 'react'
import { PropValue } from '@/lib/rpg-api/cyborg-types'

export interface PropertyValueProps extends HTMLChakraProps<'div'> {
  name: string
  propValue: PropValue
  onPropValueChanged: (value: number) => void
  onShowDetails: () => void
}

export default function PropertyValue(props: PropertyValueProps) {
  const { name, propValue, onPropValueChanged, onShowDetails } = props
  const [hide, setHide] = useState<boolean>(true)
  //const value = useMemo(() => String(propValue?.value), [propValue?.value])
  //const [open, setOpen] = useState(false)

  const propValueChanged = (e: NumberInputValueChangeDetails) => {
    if (propValue && onPropValueChanged) {
      const newBaseValue =
        propValue.baseValue + Number(e.value) - propValue.value

      onPropValueChanged(newBaseValue)
    }
  }

  let color = 'black'
  if (propValue) {
    if (propValue.baseValue > propValue.originalBaseValue) color = 'green'
    else if (propValue.baseValue < propValue.originalBaseValue) color = 'red'
  }

  return (
    <VStack>
      <Heading onClick={onShowDetails}>{name}</Heading>
      {/* <DrawerRoot open={open} onOpenChange={(e) => setOpen(e.open)}>
         <DrawerTrigger asChild>
          <Heading className={propertyValueTitle} onClick={onShowDetails}>
            {name}
          </Heading>
        </DrawerTrigger>
        <DrawerContent>
          <DrawerHeader></DrawerHeader>
          <DrawerBody>
            <p>
              Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do
              eiusmod tempor incididunt ut labore et dolore magna aliqua.
            </p>
          </DrawerBody>
          <DrawerFooter>
            <Drawer.ActionTrigger asChild>
              <Button variant="outline">Cancel</Button>
            </Drawer.ActionTrigger>
            <Button>Save</Button>
          </DrawerFooter>
          <DrawerCloseTrigger />
        </DrawerContent>
      </DrawerRoot>  */}
      <StepperInput
        hideTriggers={hide}
        spinOnPress={false}
        maxW="200px"
        color={color}
        size={'md'}
        value={String(propValue?.value)}
        onValueChange={propValueChanged}
        onMouseOver={() => setHide(false)}
        onMouseLeave={() => setHide(true)}
      />
    </VStack>
  )
}
