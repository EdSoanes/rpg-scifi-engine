'use client'

import { Box, Heading, HTMLChakraProps, VStack } from '@chakra-ui/react'
import { StepperInput } from './stepper-input'
import { useState } from 'react'
import { PropValue } from '@lib/rpg-api/cyborg-types'
import {
  DrawerActionTrigger,
  DrawerBackdrop,
  DrawerBody,
  DrawerCloseTrigger,
  DrawerContent,
  DrawerFooter,
  DrawerHeader,
  DrawerRoot,
  DrawerTitle,
  DrawerTrigger,
} from '@components/ui/drawer'
import { Button } from '@components/ui/button'
import { PropDescription } from '@lib/rpg-api/types'
import { getPropDesc } from '@lib/rpg-api/fetcher'
import { useAppSelector } from '@app/hooks'
import { selectGraphState } from '@app/graphState/graphSelectors'
import DescribeProperty from '@components/describe/DescribeProperty'

export interface PropertyValueProps extends HTMLChakraProps<'div'> {
  name: string
  prop: string
  propValue: PropValue
  onPropValueChanged: (value: number) => void
  onShowDetails: () => void
}

export default function PropertyValue(props: PropertyValueProps) {
  const { name, prop, propValue, onPropValueChanged } = props
  const [hide, setHide] = useState<boolean>(true)

  const graphState = useAppSelector(selectGraphState)
  const [loadingDescribe, setLoadingDescribe] = useState<boolean>(false)
  const [describe, setDescribe] = useState<PropDescription | null | undefined>()

  const onDescribe = async () => {
    if (graphState && !describe && prop && propValue && !loadingDescribe) {
      const response = await getPropDesc(props.propValue.id, prop, graphState)
      setDescribe(response?.data)
      setLoadingDescribe(false)
    }
  }

  const onChange = (value: string) => {
    setDescribe(undefined)
    onPropValueChanged(Number(value))
  }

  let color = 'black'
  if (propValue) {
    if (propValue.baseValue > propValue.originalBaseValue) color = 'green'
    else if (propValue.baseValue < propValue.originalBaseValue) color = 'red'
  }

  return (
    <Box
      p="4"
      borderWidth="1px"
      borderColor="border.disabled"
      color="fg.disabled"
    >
      <VStack>
        <DrawerRoot size={'lg'}>
          <DrawerBackdrop />
          <DrawerTrigger asChild>
            <Heading
              _hover={{ cursor: 'pointer' }}
              as={'h3'}
              size={'xs'}
              onClick={async () => await onDescribe()}
            >
              {name}
            </Heading>
          </DrawerTrigger>
          <DrawerContent>
            <DrawerHeader>
              <DrawerTitle>{name}</DrawerTitle>
            </DrawerHeader>
            <DrawerBody>
              <DescribeProperty description={describe} />
            </DrawerBody>
            <DrawerFooter>
              <DrawerActionTrigger asChild>
                <Button variant="outline">Cancel</Button>
              </DrawerActionTrigger>
              <Button>Save</Button>
            </DrawerFooter>
            <DrawerCloseTrigger />
          </DrawerContent>
        </DrawerRoot>
        <StepperInput
          hideTriggers={hide}
          spinOnPress={false}
          maxW="200px"
          color={color}
          size={'md'}
          value={String(propValue?.value)}
          onValueChange={(e) => onChange(e.value)}
          onMouseOver={() => setHide(false)}
          onMouseLeave={() => setHide(true)}
        />
      </VStack>
    </Box>
  )
}
