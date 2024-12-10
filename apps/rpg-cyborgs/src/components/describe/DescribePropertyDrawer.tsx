import { selectGraphState } from '@app/graphState/graphSelectors'
import { useAppSelector } from '@app/hooks'
import DescribeProperty from '@components/describe/DescribeProperty'
import { Button } from '@components/ui/button'

import {
  DrawerBackdrop,
  DrawerRoot,
  DrawerBody,
  DrawerContent,
  DrawerHeader,
  DrawerTitle,
  DrawerTrigger,
  DrawerFooter,
  DrawerActionTrigger,
  DrawerCloseTrigger,
} from '@components/ui/drawer'
import { getPropDesc } from '@lib/rpg-api/fetcher'
import { ObjectPropInfo } from '@lib/rpg-api/types'
import { useState } from 'react'

export interface DescribePropertyDrawerProps {
  entityId: string
  prop: string
  children: React.ReactNode
}

export default function DescribePropertyDrawer(
  props: DescribePropertyDrawerProps
) {
  const { entityId, prop, children } = props
  const graphState = useAppSelector(selectGraphState)
  const [describe, setDescribe] = useState<ObjectPropInfo | null | undefined>()

  const onClickedTrigger = async () => {
    if (graphState && entityId && prop) {
      const response = await getPropDesc(entityId, prop, graphState)
      setDescribe(response?.data)
    }
  }

  return (
    entityId &&
    prop && (
      <DrawerRoot size={'lg'}>
        <DrawerBackdrop />
        <DrawerTrigger asChild>
          <div onClick={async () => await onClickedTrigger()}>{children}</div>
        </DrawerTrigger>
        <DrawerContent>
          <DrawerHeader>
            <DrawerTitle>{describe?.name}</DrawerTitle>
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
    )
  )
}
